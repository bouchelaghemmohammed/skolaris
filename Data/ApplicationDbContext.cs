using Skolaris.Enums;
using Skolaris.Models;
using Microsoft.EntityFrameworkCore;

namespace Skolaris.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Ecole> Ecoles { get; set; }
    public DbSet<AnneeScolaire> AnneesScolaires { get; set; }
    public DbSet<Programme> Programmes { get; set; }
    public DbSet<Niveau> Niveaux { get; set; }
    public DbSet<Cours> Cours { get; set; }
    public DbSet<Groupe> Groupes { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Enseignant> Enseignants { get; set; }
    public DbSet<CoursOffert> CoursOfferts { get; set; }
    public DbSet<Eleve> Eleves { get; set; }
    public DbSet<Inscription> Inscriptions { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Bulletin> Bulletins { get; set; }
    public DbSet<DetailBulletin> DetailBulletins { get; set; }
    public DbSet<Absence> Absences { get; set; }
    public DbSet<JustificationAbsence> JustificationsAbsence { get; set; }
    public DbSet<EmploiDuTemps> EmploisDuTemps { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageUtilisateur> MessageUtilisateurs { get; set; }
    public DbSet<PaiementEleve> PaiementsEleves { get; set; }
    public DbSet<TauxHoraireEnseignant> TauxHorairesEnseignants { get; set; }
    public DbSet<PaiementEnseignant> PaiementsEnseignants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Utilisateur
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.IdUtilisateur);
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.MotDePasse).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.ResetPasswordToken).HasMaxLength(128);
            entity.Property(e => e.ResetPasswordTokenExpiry).IsRequired(false);
        });

        // Ecole
        modelBuilder.Entity<Ecole>(entity =>
        {
            entity.HasKey(e => e.IdEcole);
            entity.Property(e => e.Nom).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Adresse).HasMaxLength(500);
            entity.Property(e => e.Telephone).HasMaxLength(20);
        });

        // AnneeScolaire
        modelBuilder.Entity<AnneeScolaire>(entity =>
        {
            entity.HasKey(e => e.IdAnnee);
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Ecole).WithMany(e => e.AnneesScolaires).HasForeignKey(e => e.IdEcole);
        });

        // Programme
        modelBuilder.Entity<Programme>(entity =>
        {
            entity.HasKey(e => e.IdProgramme);
            entity.Property(e => e.Nom).HasMaxLength(200).IsRequired();
        });

        // Niveau
        modelBuilder.Entity<Niveau>(entity =>
        {
            entity.HasKey(e => e.IdNiveau);
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Programme).WithMany(e => e.Niveaux).HasForeignKey(e => e.IdProgramme);
        });

        // Cours
        modelBuilder.Entity<Cours>(entity =>
        {
            entity.HasKey(e => e.IdCours);
            entity.Property(e => e.Nom).HasMaxLength(200).IsRequired();
            entity.HasOne(e => e.Programme).WithMany(e => e.Cours).HasForeignKey(e => e.IdProgramme).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Niveau).WithMany(e => e.Cours).HasForeignKey(e => e.IdNiveau).OnDelete(DeleteBehavior.Restrict);
        });

        // Groupe
        modelBuilder.Entity<Groupe>(entity =>
        {
            entity.HasKey(e => e.IdGroupe);
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Programme).WithMany(e => e.Groupes).HasForeignKey(e => e.IdProgramme);
        });

        // Session
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.IdSession);
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasOne(e => e.AnneeScolaire).WithMany(e => e.Sessions).HasForeignKey(e => e.IdAnnee);
        });

        // Enseignant
        modelBuilder.Entity<Enseignant>(entity =>
        {
            entity.HasKey(e => e.IdEnseignant);
            entity.HasOne(e => e.Utilisateur).WithOne(e => e.Enseignant).HasForeignKey<Enseignant>(e => e.IdUtilisateur);
        });

        // CoursOffert
        modelBuilder.Entity<CoursOffert>(entity =>
        {
            entity.HasKey(e => e.IdCoursOffert);
            entity.Property(e => e.ModeEnseignement).HasConversion<string>();
            entity.HasOne(e => e.Cours).WithMany(e => e.CoursOfferts).HasForeignKey(e => e.IdCours);
            entity.HasOne(e => e.Groupe).WithMany(e => e.CoursOfferts).HasForeignKey(e => e.IdGroupe);
            entity.HasOne(e => e.Session).WithMany(e => e.CoursOfferts).HasForeignKey(e => e.IdSession);
            entity.HasOne(e => e.Enseignant).WithMany(e => e.CoursOfferts).HasForeignKey(e => e.IdEnseignant).IsRequired(false);
        });

        // Eleve
        modelBuilder.Entity<Eleve>(entity =>
        {
            entity.HasKey(e => e.IdEleve);
            entity.Property(e => e.Matricule).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Matricule).IsUnique();
            entity.HasOne(e => e.Utilisateur).WithOne(e => e.Eleve).HasForeignKey<Eleve>(e => e.IdUtilisateur);
            entity.HasOne(e => e.Programme).WithMany(e => e.Eleves).HasForeignKey(e => e.IdProgramme);
            entity.HasOne(e => e.Groupe).WithMany(e => e.Eleves).HasForeignKey(e => e.IdGroupe);
            entity.HasOne(e => e.Niveau).WithMany(e => e.Eleves).HasForeignKey(e => e.IdNiveau);
        });

        // Inscription
        modelBuilder.Entity<Inscription>(entity =>
        {
            entity.HasKey(e => e.IdInscription);
            entity.HasOne(e => e.Eleve).WithMany(e => e.Inscriptions).HasForeignKey(e => e.IdEleve);
            entity.HasOne(e => e.CoursOffert).WithMany(e => e.Inscriptions).HasForeignKey(e => e.IdCoursOffert);
        });

        // Note
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.IdNote);
            entity.Property(e => e.Valeur).HasPrecision(4, 2);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasOne(e => e.Eleve).WithMany(e => e.Notes).HasForeignKey(e => e.IdEleve);
            entity.HasOne(e => e.CoursOffert).WithMany(e => e.Notes).HasForeignKey(e => e.IdCoursOffert);
        });

        // Bulletin
        modelBuilder.Entity<Bulletin>(entity =>
        {
            entity.HasKey(e => e.IdBulletin);
            entity.Property(e => e.MoyenneGeneral).HasPrecision(4, 2);
            entity.Property(e => e.Mention).HasConversion<string>();
            entity.HasOne(e => e.Eleve).WithMany(e => e.Bulletins).HasForeignKey(e => e.IdEleve);
            entity.HasOne(e => e.Session).WithMany(e => e.Bulletins).HasForeignKey(e => e.IdSession);
        });

        // DetailBulletin
        modelBuilder.Entity<DetailBulletin>(entity =>
        {
            entity.HasKey(e => e.IdDetailBulletin);
            entity.HasOne(e => e.Bulletin).WithMany(e => e.DetailBulletins).HasForeignKey(e => e.IdBulletin);
            entity.HasOne(e => e.Note).WithMany(e => e.DetailBulletins).HasForeignKey(e => e.IdNote);
            entity.HasOne(e => e.CoursOffert).WithMany(e => e.DetailBulletins).HasForeignKey(e => e.IdCoursOffert);
        });

        // Absence
        modelBuilder.Entity<Absence>(entity =>
        {
            entity.HasKey(e => e.IdAbsence);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.HasOne(e => e.Eleve).WithMany(e => e.Absences).HasForeignKey(e => e.IdEleve);
            entity.HasOne(e => e.CoursOffert).WithMany(e => e.Absences).HasForeignKey(e => e.IdCoursOffert);
        });

        // JustificationAbsence
        modelBuilder.Entity<JustificationAbsence>(entity =>
        {
            entity.HasKey(e => e.IdJustification);
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.HasOne(e => e.Absence).WithOne(e => e.Justification).HasForeignKey<JustificationAbsence>(e => e.IdAbsence);
        });

        // EmploiDuTemps
        modelBuilder.Entity<EmploiDuTemps>(entity =>
        {
            entity.HasKey(e => e.IdEmploi);
            entity.Property(e => e.JourSemaine).HasConversion<string>();
            entity.HasOne(e => e.CoursOffert).WithMany(e => e.EmploisDuTemps).HasForeignKey(e => e.IdCoursOffert);
        });

        // Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.IdConversation);
            entity.Property(e => e.Sujet).HasMaxLength(300);
            entity.Property(e => e.Type).HasConversion<string>();
        });

        // ConversationParticipant
        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => new { e.IdConversation, e.IdUtilisateur });
            entity.HasOne(e => e.Conversation).WithMany(e => e.Participants).HasForeignKey(e => e.IdConversation);
            entity.HasOne(e => e.Utilisateur).WithMany(e => e.ConversationParticipants).HasForeignKey(e => e.IdUtilisateur);
        });

        // Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage);
            entity.Property(e => e.Contenu).HasMaxLength(4000);
            entity.HasOne(e => e.Conversation).WithMany(e => e.Messages).HasForeignKey(e => e.IdConversation);
            entity.HasOne(e => e.Expediteur).WithMany(e => e.MessagesEnvoyes).HasForeignKey(e => e.IdExpediteur);
        });

        // MessageUtilisateur
        modelBuilder.Entity<MessageUtilisateur>(entity =>
        {
            entity.HasKey(e => e.IdMessageUtilisateur);
            entity.HasOne(e => e.Message).WithMany(e => e.MessageUtilisateurs).HasForeignKey(e => e.IdMessage);
        });

        // PaiementEleve
        modelBuilder.Entity<PaiementEleve>(entity =>
        {
            entity.HasKey(e => e.IdPaiementEleve);
            entity.Property(e => e.Montant).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.DatePaiement).IsRequired();
            entity.Property(e => e.ModePaiement).HasMaxLength(50);
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.HasOne(e => e.Eleve).WithMany(e => e.Paiements).HasForeignKey(e => e.IdEleve);
            entity.HasOne(e => e.Session).WithMany(e => e.PaiementsEleves).HasForeignKey(e => e.IdSession);
        });

        // TauxHoraireEnseignant
        modelBuilder.Entity<TauxHoraireEnseignant>(entity =>
        {
            entity.HasKey(e => e.IdTaux);
            entity.Property(e => e.TauxHoraire).HasPrecision(8, 2).IsRequired();
            entity.Property(e => e.DateEffet).IsRequired();
            entity.HasOne(e => e.Enseignant).WithMany(e => e.TauxHoraires).HasForeignKey(e => e.IdEnseignant);
        });

        // PaiementEnseignant
        modelBuilder.Entity<PaiementEnseignant>(entity =>
        {
            entity.HasKey(e => e.IdPaiementEnseignant);
            entity.Property(e => e.HeuresTravaillees).HasPrecision(6, 2).IsRequired();
            entity.Property(e => e.MontantTotal).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.DatePaiement).IsRequired();
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.HasOne(e => e.Enseignant).WithMany(e => e.Paiements).HasForeignKey(e => e.IdEnseignant);
            entity.HasOne(e => e.Session).WithMany(e => e.PaiementsEnseignants).HasForeignKey(e => e.IdSession);
            entity.HasOne(e => e.TauxHoraire).WithMany(e => e.Paiements).HasForeignKey(e => e.IdTaux);
        });

        // Désactiver la suppression en cascade globalement (évite les erreurs SQL Server)
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
