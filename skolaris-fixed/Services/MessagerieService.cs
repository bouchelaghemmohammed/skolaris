using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class MessagerieService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<MessagerieService> _logger;

        public MessagerieService(ApplicationDbContext context, EmailService emailService, ILogger<MessagerieService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // Retourne toutes les conversations d'un utilisateur avec le dernier message et le nb non lus
        public List<ConversationSummaryVm> GetConversations(int userId)
        {
            return _context.ConversationParticipants
                .Where(cp => cp.IdUtilisateur == userId)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(m => m.MessageUtilisateurs)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Participants)
                        .ThenInclude(p => p.Utilisateur)
                .Select(cp => new ConversationSummaryVm
                {
                    IdConversation = cp.IdConversation,
                    Sujet = cp.Conversation.Sujet ?? "(sans sujet)",
                    Type = cp.Conversation.Type.ToString(),
                    EstAnnonce = cp.Conversation.EstAnnonce,
                    DateCreation = cp.Conversation.DateCreation,
                    DernierMessage = cp.Conversation.Messages
                        .OrderByDescending(m => m.DateEnvoi)
                        .Select(m => m.Contenu)
                        .FirstOrDefault(),
                    DateDernierMessage = cp.Conversation.Messages
                        .OrderByDescending(m => m.DateEnvoi)
                        .Select(m => (DateTime?)m.DateEnvoi)
                        .FirstOrDefault(),
                    NombreNonLus = cp.Conversation.Messages
                        .SelectMany(m => m.MessageUtilisateurs)
                        .Count(mu => mu.IdDestinataire == userId && !mu.EstLu),
                    Participants = cp.Conversation.Participants
                        .Select(p => new ParticipantVm
                        {
                            IdUtilisateur = p.IdUtilisateur,
                            Nom = p.Utilisateur.Prenom + " " + p.Utilisateur.Nom
                        })
                        .ToList()
                })
                .OrderByDescending(c => c.DateDernierMessage ?? c.DateCreation)
                .ToList();
        }

        // Retourne les messages d'une conversation
        public List<MessageVm> GetMessages(int conversationId, int userId)
        {
            // Vérifier que l'utilisateur est participant
            var isParticipant = _context.ConversationParticipants
                .Any(cp => cp.IdConversation == conversationId && cp.IdUtilisateur == userId);
            if (!isParticipant) return new List<MessageVm>();

            return _context.Messages
                .Where(m => m.IdConversation == conversationId)
                .Include(m => m.Expediteur)
                .Include(m => m.MessageUtilisateurs)
                .OrderBy(m => m.DateEnvoi)
                .Select(m => new MessageVm
                {
                    IdMessage = m.IdMessage,
                    Contenu = m.Contenu ?? "",
                    DateEnvoi = m.DateEnvoi,
                    IdExpediteur = m.IdExpediteur,
                    NomExpediteur = m.Expediteur.Prenom + " " + m.Expediteur.Nom,
                    PieceJointeNom = m.PieceJointeNom,
                    PieceJointePath = m.PieceJointePath,
                    EstSignale = m.EstSignale,
                    EstLu = m.MessageUtilisateurs
                        .Where(mu => mu.IdDestinataire == userId)
                        .Select(mu => mu.EstLu)
                        .FirstOrDefault()
                })
                .ToList();
        }

        // Crée une nouvelle conversation privée ou de groupe
        public int CreateConversation(int creatorId, string sujet, TypeConversation type, List<int> participantIds, bool estAnnonce = false)
        {
            // Valider que le créateur existe (évite le crash FK si session expirée)
            if (!_context.Utilisateurs.Any(u => u.IdUtilisateur == creatorId))
                return -1;

            var conv = new Conversation
            {
                Sujet = sujet,
                Type = type,
                DateCreation = DateTime.UtcNow,
                IdCreateur = creatorId,
                EstAnnonce = estAnnonce
            };
            _context.Conversations.Add(conv);
            _context.SaveChanges();

            // Ajouter le créateur + les participants (seulement les IDs valides)
            var allIds = participantIds.Distinct().ToList();
            if (!allIds.Contains(creatorId)) allIds.Add(creatorId);

            var validIds = _context.Utilisateurs
                .Where(u => allIds.Contains(u.IdUtilisateur))
                .Select(u => u.IdUtilisateur)
                .ToList();

            foreach (var uid in validIds)
            {
                _context.ConversationParticipants.Add(new ConversationParticipant
                {
                    IdConversation = conv.IdConversation,
                    IdUtilisateur = uid,
                    DateAdhesion = DateTime.UtcNow
                });
            }
            _context.SaveChanges();
            return conv.IdConversation;
        }

        // Envoie un message dans une conversation existante
        public int SendMessage(int conversationId, int senderId, string contenu, string? pieceJointePath = null, string? pieceJointeNom = null)
        {
            var isParticipant = _context.ConversationParticipants
                .Any(cp => cp.IdConversation == conversationId && cp.IdUtilisateur == senderId);
            if (!isParticipant) return -1;

            var msg = new Message
            {
                IdConversation = conversationId,
                IdExpediteur = senderId,
                Contenu = contenu,
                DateEnvoi = DateTime.UtcNow,
                PieceJointePath = pieceJointePath,
                PieceJointeNom = pieceJointeNom
            };
            _context.Messages.Add(msg);
            _context.SaveChanges();

            // Créer les entrées MessageUtilisateur pour chaque autre participant
            var otherParticipants = _context.ConversationParticipants
                .Where(cp => cp.IdConversation == conversationId && cp.IdUtilisateur != senderId)
                .Select(cp => cp.IdUtilisateur)
                .ToList();

            foreach (var uid in otherParticipants)
            {
                _context.MessageUtilisateurs.Add(new MessageUtilisateur
                {
                    IdMessage = msg.IdMessage,
                    IdDestinataire = uid,
                    EstLu = false
                });
            }
            _context.SaveChanges();

            // Notification email asynchrone — données récupérées AVANT Task.Run
            // car _context (Scoped) est disposé après la requête HTTP
            var destinatairesEmail = _context.Utilisateurs
                .Where(u => otherParticipants.Contains(u.IdUtilisateur))
                .Select(u => new { u.Email, u.Prenom, u.Nom })
                .ToList();

            if (destinatairesEmail.Any())
            {
                var sujetConv = _context.Conversations
                    .Where(c => c.IdConversation == conversationId)
                    .Select(c => c.Sujet ?? "Nouveau message")
                    .FirstOrDefault() ?? "Nouveau message";

                var contenuCopie = contenu;
                var emailService = _emailService;
                var logger = _logger;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        foreach (var d in destinatairesEmail)
                        {
                            var htmlBody = $@"
                                <p>Bonjour {d.Prenom},</p>
                                <p>Vous avez reçu un nouveau message sur <strong>Skolaris</strong>.</p>
                                <p style='color:#555;'>Sujet : <em>{System.Net.WebUtility.HtmlEncode(sujetConv)}</em></p>
                                <blockquote style='border-left:3px solid #1976d2;padding:8px 16px;color:#333;background:#f0f4ff;border-radius:4px;'>
                                    {System.Net.WebUtility.HtmlEncode(contenuCopie.Length > 200 ? contenuCopie.Substring(0, 200) + "..." : contenuCopie)}
                                </blockquote>
                                <p>Connectez-vous à Skolaris pour consulter et répondre.</p>";
                            await emailService.SendEmailAsync(d.Email, $"{d.Prenom} {d.Nom}", $"📬 Nouveau message — {sujetConv}", htmlBody);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Erreur notification email messagerie: {Msg}", ex.Message);
                    }
                });
            }

            return msg.IdMessage;
        }

        // Marquer un message comme lu
        public bool MarquerLu(int messageId, int userId)
        {
            var mu = _context.MessageUtilisateurs
                .FirstOrDefault(mu => mu.IdMessage == messageId && mu.IdDestinataire == userId);
            if (mu == null) return false;
            mu.EstLu = true;
            mu.DateLecture = DateTime.UtcNow;
            _context.SaveChanges();
            return true;
        }

        // Marquer un message comme non lu
        public bool MarquerNonLu(int messageId, int userId)
        {
            var mu = _context.MessageUtilisateurs
                .FirstOrDefault(mu => mu.IdMessage == messageId && mu.IdDestinataire == userId);
            if (mu == null) return false;
            mu.EstLu = false;
            mu.DateLecture = null;
            _context.SaveChanges();
            return true;
        }

        // Marquer tous les messages d'une conversation comme lus
        public void MarquerTousLus(int conversationId, int userId)
        {
            var messageIds = _context.Messages
                .Where(m => m.IdConversation == conversationId)
                .Select(m => m.IdMessage)
                .ToList();

            var nonLus = _context.MessageUtilisateurs
                .Where(mu => messageIds.Contains(mu.IdMessage) && mu.IdDestinataire == userId && !mu.EstLu)
                .ToList();

            foreach (var mu in nonLus)
            {
                mu.EstLu = true;
                mu.DateLecture = DateTime.UtcNow;
            }
            _context.SaveChanges();
        }

        // Nombre de messages non lus pour un utilisateur
        public int GetNonLuCount(int userId)
        {
            return _context.MessageUtilisateurs
                .Count(mu => mu.IdDestinataire == userId && !mu.EstLu);
        }

        // Signaler un message (n'importe quel participant)
        public bool SignalerMessage(int messageId)
        {
            var msg = _context.Messages.FirstOrDefault(m => m.IdMessage == messageId);
            if (msg == null) return false;
            msg.EstSignale = true;
            _context.SaveChanges();
            return true;
        }

        // Admin: liste des messages signalés
        public List<MessageSignaleVm> GetMessagesSignales()
        {
            return _context.Messages
                .Where(m => m.EstSignale)
                .Include(m => m.Expediteur)
                .Include(m => m.Conversation)
                .OrderByDescending(m => m.DateEnvoi)
                .Select(m => new MessageSignaleVm
                {
                    IdMessage = m.IdMessage,
                    Contenu = m.Contenu ?? "",
                    DateEnvoi = m.DateEnvoi,
                    NomExpediteur = m.Expediteur.Prenom + " " + m.Expediteur.Nom,
                    SujetConversation = m.Conversation.Sujet ?? "(sans sujet)"
                })
                .ToList();
        }

        // Admin: supprimer un message signalé (modération)
        public bool SupprimerMessage(int messageId)
        {
            var msg = _context.Messages
                .Include(m => m.MessageUtilisateurs)
                .FirstOrDefault(m => m.IdMessage == messageId);
            if (msg == null) return false;

            _context.MessageUtilisateurs.RemoveRange(msg.MessageUtilisateurs);
            _context.Messages.Remove(msg);
            _context.SaveChanges();
            return true;
        }

        // Admin: lever le signalement
        public bool LeverSignalement(int messageId)
        {
            var msg = _context.Messages.FirstOrDefault(m => m.IdMessage == messageId);
            if (msg == null) return false;
            msg.EstSignale = false;
            _context.SaveChanges();
            return true;
        }

        // Admin: annonce aux utilisateurs actifs (cible: TOUS, ENSEIGNANTS ou ELEVES)
        public int EnvoyerAnnonce(int adminId, string sujet, string contenu, string cible = "TOUS")
        {
            var query = _context.Utilisateurs.Where(u => u.IsActive);

            if (cible == "ENSEIGNANTS")
                query = query.Where(u => u.Role == Role.ENSEIGNANT);
            else if (cible == "ELEVES")
                query = query.Where(u => u.Role == Role.ELEVE);
            // TOUS = pas de filtre de rôle

            var destIds = query.Select(u => u.IdUtilisateur).ToList();

            var convId = CreateConversation(adminId, sujet, TypeConversation.Groupe, destIds, estAnnonce: true);
            SendMessage(convId, adminId, contenu);
            return convId;
        }

        // Enseignant: envoyer à tous les élèves d'un groupe
        public int EnvoyerAuGroupe(int enseignantId, int groupeId, string sujet, string contenu)
        {
            var eleveIds = _context.Eleves
                .Where(e => e.IdGroupe == groupeId)
                .Select(e => e.IdUtilisateur)
                .ToList();

            if (!eleveIds.Any()) return -1;

            var convId = CreateConversation(enseignantId, sujet, TypeConversation.Groupe, eleveIds);
            SendMessage(convId, enseignantId, contenu);
            return convId;
        }

        // Liste des utilisateurs actifs (pour créer une nouvelle conversation)
        public List<UtilisateurSimpleVm> GetUtilisateursActifs(int currentUserId)
        {
            return _context.Utilisateurs
                .Where(u => u.IsActive && u.IdUtilisateur != currentUserId)
                .Select(u => new UtilisateurSimpleVm
                {
                    IdUtilisateur = u.IdUtilisateur,
                    Nom = u.Prenom + " " + u.Nom,
                    Role = u.Role.ToString()
                })
                .OrderBy(u => u.Nom)
                .ToList();
        }

        // Liste des groupes (pour enseignant)
        public List<GroupeSimpleVm> GetGroupes()
        {
            return _context.Groupes
                .Select(g => new GroupeSimpleVm
                {
                    IdGroupe = g.IdGroupe,
                    Nom = g.Nom
                })
                .OrderBy(g => g.Nom)
                .ToList();
        }

    }

    // ── ViewModels internes ──────────────────────────────────────────────
    public class ConversationSummaryVm
    {
        public int IdConversation { get; set; }
        public string Sujet { get; set; } = "";
        public string Type { get; set; } = "";
        public bool EstAnnonce { get; set; }
        public DateTime DateCreation { get; set; }
        public string? DernierMessage { get; set; }
        public DateTime? DateDernierMessage { get; set; }
        public int NombreNonLus { get; set; }
        public List<ParticipantVm> Participants { get; set; } = new();
    }

    public class ParticipantVm
    {
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; } = "";
    }

    public class MessageVm
    {
        public int IdMessage { get; set; }
        public string Contenu { get; set; } = "";
        public DateTime DateEnvoi { get; set; }
        public int IdExpediteur { get; set; }
        public string NomExpediteur { get; set; } = "";
        public string? PieceJointeNom { get; set; }
        public string? PieceJointePath { get; set; }
        public bool EstSignale { get; set; }
        public bool EstLu { get; set; }
    }

    public class MessageSignaleVm
    {
        public int IdMessage { get; set; }
        public string Contenu { get; set; } = "";
        public DateTime DateEnvoi { get; set; }
        public string NomExpediteur { get; set; } = "";
        public string SujetConversation { get; set; } = "";
    }

    public class UtilisateurSimpleVm
    {
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class GroupeSimpleVm
    {
        public int IdGroupe { get; set; }
        public string Nom { get; set; } = "";
    }
}
