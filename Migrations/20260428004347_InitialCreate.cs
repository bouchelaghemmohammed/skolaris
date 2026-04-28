using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skolaris.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    IdConversation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sujet = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.IdConversation);
                });

            migrationBuilder.CreateTable(
                name: "Ecoles",
                columns: table => new
                {
                    IdEcole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecoles", x => x.IdEcole);
                });

            migrationBuilder.CreateTable(
                name: "Programmes",
                columns: table => new
                {
                    IdProgramme = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmes", x => x.IdProgramme);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ResetPasswordToken = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                });

            migrationBuilder.CreateTable(
                name: "AnneesScolaires",
                columns: table => new
                {
                    IdAnnee = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnneesScolaires", x => x.IdAnnee);
                    table.ForeignKey(
                        name: "FK_AnneesScolaires_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Groupes",
                columns: table => new
                {
                    IdGroupe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdProgramme = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groupes", x => x.IdGroupe);
                    table.ForeignKey(
                        name: "FK_Groupes_Programmes_IdProgramme",
                        column: x => x.IdProgramme,
                        principalTable: "Programmes",
                        principalColumn: "IdProgramme",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Niveaux",
                columns: table => new
                {
                    IdNiveau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdProgramme = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Niveaux", x => x.IdNiveau);
                    table.ForeignKey(
                        name: "FK_Niveaux_Programmes_IdProgramme",
                        column: x => x.IdProgramme,
                        principalTable: "Programmes",
                        principalColumn: "IdProgramme",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    IdConversation = table.Column<int>(type: "int", nullable: false),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    DateAdhesion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => new { x.IdConversation, x.IdUtilisateur });
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_IdConversation",
                        column: x => x.IdConversation,
                        principalTable: "Conversations",
                        principalColumn: "IdConversation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enseignants",
                columns: table => new
                {
                    IdEnseignant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enseignants", x => x.IdEnseignant);
                    table.ForeignKey(
                        name: "FK_Enseignants_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    IdMessage = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DateEnvoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdConversation = table.Column<int>(type: "int", nullable: false),
                    IdExpediteur = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.IdMessage);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_IdConversation",
                        column: x => x.IdConversation,
                        principalTable: "Conversations",
                        principalColumn: "IdConversation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Utilisateurs_IdExpediteur",
                        column: x => x.IdExpediteur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    IdSession = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdAnnee = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.IdSession);
                    table.ForeignKey(
                        name: "FK_Sessions_AnneesScolaires_IdAnnee",
                        column: x => x.IdAnnee,
                        principalTable: "AnneesScolaires",
                        principalColumn: "IdAnnee",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cours",
                columns: table => new
                {
                    IdCours = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdProgramme = table.Column<int>(type: "int", nullable: false),
                    IdNiveau = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cours", x => x.IdCours);
                    table.ForeignKey(
                        name: "FK_Cours_Niveaux_IdNiveau",
                        column: x => x.IdNiveau,
                        principalTable: "Niveaux",
                        principalColumn: "IdNiveau",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cours_Programmes_IdProgramme",
                        column: x => x.IdProgramme,
                        principalTable: "Programmes",
                        principalColumn: "IdProgramme",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Eleves",
                columns: table => new
                {
                    IdEleve = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    IdProgramme = table.Column<int>(type: "int", nullable: false),
                    IdGroupe = table.Column<int>(type: "int", nullable: false),
                    IdNiveau = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eleves", x => x.IdEleve);
                    table.ForeignKey(
                        name: "FK_Eleves_Groupes_IdGroupe",
                        column: x => x.IdGroupe,
                        principalTable: "Groupes",
                        principalColumn: "IdGroupe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eleves_Niveaux_IdNiveau",
                        column: x => x.IdNiveau,
                        principalTable: "Niveaux",
                        principalColumn: "IdNiveau",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eleves_Programmes_IdProgramme",
                        column: x => x.IdProgramme,
                        principalTable: "Programmes",
                        principalColumn: "IdProgramme",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eleves_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TauxHorairesEnseignants",
                columns: table => new
                {
                    IdTaux = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TauxHoraire = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    DateEffet = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdEnseignant = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TauxHorairesEnseignants", x => x.IdTaux);
                    table.ForeignKey(
                        name: "FK_TauxHorairesEnseignants_Enseignants_IdEnseignant",
                        column: x => x.IdEnseignant,
                        principalTable: "Enseignants",
                        principalColumn: "IdEnseignant",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageUtilisateurs",
                columns: table => new
                {
                    IdMessageUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMessage = table.Column<int>(type: "int", nullable: false),
                    IdDestinataire = table.Column<int>(type: "int", nullable: false),
                    EstLu = table.Column<bool>(type: "bit", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageUtilisateurs", x => x.IdMessageUtilisateur);
                    table.ForeignKey(
                        name: "FK_MessageUtilisateurs_Messages_IdMessage",
                        column: x => x.IdMessage,
                        principalTable: "Messages",
                        principalColumn: "IdMessage",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoursOfferts",
                columns: table => new
                {
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCours = table.Column<int>(type: "int", nullable: false),
                    IdGroupe = table.Column<int>(type: "int", nullable: false),
                    IdSession = table.Column<int>(type: "int", nullable: false),
                    IdEnseignant = table.Column<int>(type: "int", nullable: true),
                    ModeEnseignement = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursOfferts", x => x.IdCoursOffert);
                    table.ForeignKey(
                        name: "FK_CoursOfferts_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursOfferts_Enseignants_IdEnseignant",
                        column: x => x.IdEnseignant,
                        principalTable: "Enseignants",
                        principalColumn: "IdEnseignant",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursOfferts_Groupes_IdGroupe",
                        column: x => x.IdGroupe,
                        principalTable: "Groupes",
                        principalColumn: "IdGroupe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursOfferts_Sessions_IdSession",
                        column: x => x.IdSession,
                        principalTable: "Sessions",
                        principalColumn: "IdSession",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bulletins",
                columns: table => new
                {
                    IdBulletin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoyenneGeneral = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Mention = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdSession = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bulletins", x => x.IdBulletin);
                    table.ForeignKey(
                        name: "FK_Bulletins_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bulletins_Sessions_IdSession",
                        column: x => x.IdSession,
                        principalTable: "Sessions",
                        principalColumn: "IdSession",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaiementsEleves",
                columns: table => new
                {
                    IdPaiementEleve = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Montant = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModePaiement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdSession = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsEleves", x => x.IdPaiementEleve);
                    table.ForeignKey(
                        name: "FK_PaiementsEleves_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaiementsEleves_Sessions_IdSession",
                        column: x => x.IdSession,
                        principalTable: "Sessions",
                        principalColumn: "IdSession",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaiementsEnseignants",
                columns: table => new
                {
                    IdPaiementEnseignant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeuresTravaillees = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdEnseignant = table.Column<int>(type: "int", nullable: false),
                    IdSession = table.Column<int>(type: "int", nullable: false),
                    IdTaux = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsEnseignants", x => x.IdPaiementEnseignant);
                    table.ForeignKey(
                        name: "FK_PaiementsEnseignants_Enseignants_IdEnseignant",
                        column: x => x.IdEnseignant,
                        principalTable: "Enseignants",
                        principalColumn: "IdEnseignant",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaiementsEnseignants_Sessions_IdSession",
                        column: x => x.IdSession,
                        principalTable: "Sessions",
                        principalColumn: "IdSession",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaiementsEnseignants_TauxHorairesEnseignants_IdTaux",
                        column: x => x.IdTaux,
                        principalTable: "TauxHorairesEnseignants",
                        principalColumn: "IdTaux",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    IdAbsence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAbsence = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.IdAbsence);
                    table.ForeignKey(
                        name: "FK_Absences_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Absences_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmploisDuTemps",
                columns: table => new
                {
                    IdEmploi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JourSemaine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeureDebut = table.Column<TimeSpan>(type: "time", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Salle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploisDuTemps", x => x.IdEmploi);
                    table.ForeignKey(
                        name: "FK_EmploisDuTemps_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscriptions",
                columns: table => new
                {
                    IdInscription = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscriptions", x => x.IdInscription);
                    table.ForeignKey(
                        name: "FK_Inscriptions_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscriptions_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    IdNote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valeur = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.IdNote);
                    table.ForeignKey(
                        name: "FK_Notes_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JustificationsAbsence",
                columns: table => new
                {
                    IdJustification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdAbsence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JustificationsAbsence", x => x.IdJustification);
                    table.ForeignKey(
                        name: "FK_JustificationsAbsence_Absences_IdAbsence",
                        column: x => x.IdAbsence,
                        principalTable: "Absences",
                        principalColumn: "IdAbsence",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetailBulletins",
                columns: table => new
                {
                    IdDetailBulletin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdBulletin = table.Column<int>(type: "int", nullable: false),
                    IdNote = table.Column<int>(type: "int", nullable: false),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailBulletins", x => x.IdDetailBulletin);
                    table.ForeignKey(
                        name: "FK_DetailBulletins_Bulletins_IdBulletin",
                        column: x => x.IdBulletin,
                        principalTable: "Bulletins",
                        principalColumn: "IdBulletin",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetailBulletins_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetailBulletins_Notes_IdNote",
                        column: x => x.IdNote,
                        principalTable: "Notes",
                        principalColumn: "IdNote",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Absences_IdCoursOffert",
                table: "Absences",
                column: "IdCoursOffert");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_IdEleve",
                table: "Absences",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_AnneesScolaires_IdEcole",
                table: "AnneesScolaires",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_IdEleve",
                table: "Bulletins",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_IdSession",
                table: "Bulletins",
                column: "IdSession");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_IdUtilisateur",
                table: "ConversationParticipants",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Cours_IdNiveau",
                table: "Cours",
                column: "IdNiveau");

            migrationBuilder.CreateIndex(
                name: "IX_Cours_IdProgramme",
                table: "Cours",
                column: "IdProgramme");

            migrationBuilder.CreateIndex(
                name: "IX_CoursOfferts_IdCours",
                table: "CoursOfferts",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_CoursOfferts_IdEnseignant",
                table: "CoursOfferts",
                column: "IdEnseignant");

            migrationBuilder.CreateIndex(
                name: "IX_CoursOfferts_IdGroupe",
                table: "CoursOfferts",
                column: "IdGroupe");

            migrationBuilder.CreateIndex(
                name: "IX_CoursOfferts_IdSession",
                table: "CoursOfferts",
                column: "IdSession");

            migrationBuilder.CreateIndex(
                name: "IX_DetailBulletins_IdBulletin",
                table: "DetailBulletins",
                column: "IdBulletin");

            migrationBuilder.CreateIndex(
                name: "IX_DetailBulletins_IdCoursOffert",
                table: "DetailBulletins",
                column: "IdCoursOffert");

            migrationBuilder.CreateIndex(
                name: "IX_DetailBulletins_IdNote",
                table: "DetailBulletins",
                column: "IdNote");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdGroupe",
                table: "Eleves",
                column: "IdGroupe");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdNiveau",
                table: "Eleves",
                column: "IdNiveau");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdProgramme",
                table: "Eleves",
                column: "IdProgramme");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdUtilisateur",
                table: "Eleves",
                column: "IdUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_Matricule",
                table: "Eleves",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploisDuTemps_IdCoursOffert",
                table: "EmploisDuTemps",
                column: "IdCoursOffert");

            migrationBuilder.CreateIndex(
                name: "IX_Enseignants_IdUtilisateur",
                table: "Enseignants",
                column: "IdUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_IdProgramme",
                table: "Groupes",
                column: "IdProgramme");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdCoursOffert",
                table: "Inscriptions",
                column: "IdCoursOffert");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdEleve",
                table: "Inscriptions",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_JustificationsAbsence_IdAbsence",
                table: "JustificationsAbsence",
                column: "IdAbsence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdConversation",
                table: "Messages",
                column: "IdConversation");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdExpediteur",
                table: "Messages",
                column: "IdExpediteur");

            migrationBuilder.CreateIndex(
                name: "IX_MessageUtilisateurs_IdMessage",
                table: "MessageUtilisateurs",
                column: "IdMessage");

            migrationBuilder.CreateIndex(
                name: "IX_Niveaux_IdProgramme",
                table: "Niveaux",
                column: "IdProgramme");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdCoursOffert",
                table: "Notes",
                column: "IdCoursOffert");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdEleve",
                table: "Notes",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsEleves_IdEleve",
                table: "PaiementsEleves",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsEleves_IdSession",
                table: "PaiementsEleves",
                column: "IdSession");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsEnseignants_IdEnseignant",
                table: "PaiementsEnseignants",
                column: "IdEnseignant");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsEnseignants_IdSession",
                table: "PaiementsEnseignants",
                column: "IdSession");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsEnseignants_IdTaux",
                table: "PaiementsEnseignants",
                column: "IdTaux");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_IdAnnee",
                table: "Sessions",
                column: "IdAnnee");

            migrationBuilder.CreateIndex(
                name: "IX_TauxHorairesEnseignants_IdEnseignant",
                table: "TauxHorairesEnseignants",
                column: "IdEnseignant");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "DetailBulletins");

            migrationBuilder.DropTable(
                name: "EmploisDuTemps");

            migrationBuilder.DropTable(
                name: "Inscriptions");

            migrationBuilder.DropTable(
                name: "JustificationsAbsence");

            migrationBuilder.DropTable(
                name: "MessageUtilisateurs");

            migrationBuilder.DropTable(
                name: "PaiementsEleves");

            migrationBuilder.DropTable(
                name: "PaiementsEnseignants");

            migrationBuilder.DropTable(
                name: "Bulletins");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Absences");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "TauxHorairesEnseignants");

            migrationBuilder.DropTable(
                name: "CoursOfferts");

            migrationBuilder.DropTable(
                name: "Eleves");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Cours");

            migrationBuilder.DropTable(
                name: "Enseignants");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Groupes");

            migrationBuilder.DropTable(
                name: "Niveaux");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "AnneesScolaires");

            migrationBuilder.DropTable(
                name: "Programmes");

            migrationBuilder.DropTable(
                name: "Ecoles");
        }
    }
}
