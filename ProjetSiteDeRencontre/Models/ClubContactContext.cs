/*------------------------------------------------------------------------------------

CLASSE DE CONTEXTE DU PROJET POUR LA BASE DE DONNÉES
DES RELATIONS ONT ÉTÉ CONFIGURÉS MANUELLEMENT AVEC FLUENT API

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Data.Entity;
using System.Linq;
using ProjetSiteDeRencontre.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ProjetSiteDeRencontre.Models
{
    public class ClubContactContext : DbContext
    {
        public ClubContactContext()
            : base("name=ClubContactContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            
            modelBuilder.Entity<Membre>()
                .HasMany(favoris => favoris.listeFavoris)
                .WithMany(favoris => favoris.listeMembreQuiMontFavoriser)
                .Map(m =>
                {
                    m.MapLeftKey("noMembreEnvoyeur");
                    m.MapRightKey("noMembreReceveur");
                    m.ToTable("FavorisAvec");
                });

            modelBuilder.Entity<Membre>()
                .HasMany(listeNoire => listeNoire.listeNoire)
                .WithMany(listeMembresQuiMontBloque => listeMembresQuiMontBloque.listeMembresQuiMontBloque)
                .Map(m =>
                {
                    m.MapLeftKey("noMembreBloqueur");
                    m.MapRightKey("noMembreBloquer");
                    m.ToTable("BloqueMembre");
                });

            modelBuilder.Entity<Membre>()
                .HasMany(contacts => contacts.listeContacts)
                .WithMany()
                .Map(m =>
                {
                    m.MapLeftKey("noMembreAyantContact");
                    m.MapRightKey("noMembreQuiEstDansContact");
                    m.ToTable("ContactMembre");
                });

            modelBuilder.Entity<Membre>()
                .HasRequired(mem => mem.province)
                .WithMany()
                .HasForeignKey(m => m.noProvince)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Membre>()
                .HasRequired(mem => mem.ville)
                .WithMany()
                .HasForeignKey(m => m.noVille)
                .WillCascadeOnDelete(false);


            //Visites

            modelBuilder.Entity<Visite>()
                .HasRequired(visite => visite.membreVisite)
                .WithMany(m => m.listeDeVisitesDeMonProfil)
                .HasForeignKey(m => m.noMembreVisite)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Visite>()
                .HasRequired(visite => visite.membreVisiteur)
                .WithMany(visite => visite.listeDesVisitesQueJaiFait)
                .HasForeignKey(m => m.noMembreVisiteur)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Membre>()
            //    .HasMany(mem => mem.listeActivitesOrganises)
            //    .WithRequired(/*m => m.membreOrganisateur*/)
            //    .WillCascadeOnDelete(true);

            //Activités

            modelBuilder.Entity<Activite>()
                .HasRequired(membre => membre.membreOrganisateur)
                .WithMany(activite => activite.listeActivitesOrganises)
                .HasForeignKey(m => m.noMembreOrganisateur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Activite>()
                .HasMany(membres => membres.membresParticipants)
                .WithMany(activites => activites.listeActivites);

            modelBuilder.Entity<Activite>()
                .HasRequired(ac => ac.province)
                .WithMany()
                .HasForeignKey(a => a.noProvince)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Activite>()
                .HasRequired(a => a.ville)
                .WithMany()
                .HasForeignKey(ac => ac.noVille)
                .WillCascadeOnDelete(false);

            //Messages
            modelBuilder.Entity<Message>()
                .HasOptional(m => m.membreEnvoyeur)
                .WithMany(mem => mem.listeMessagesEnvoyes)
                .HasForeignKey(t => t.noMembreEnvoyeur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.membreReceveur)
                .WithMany(mem => mem.listeMessagesRecus)
                .HasForeignKey(t => t.noMembreReceveur)
                .WillCascadeOnDelete(false);

            //Cadeaux
            modelBuilder.Entity<Gift>()
                .HasRequired(m => m.membreEnvoyeur)
                .WithMany(mem => mem.listeCadeauxEnvoyes)
                .HasForeignKey(t => t.noMembreEnvoyeur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Gift>()
                .HasRequired(m => m.membreReceveur)
                .WithMany(mem => mem.listeCadeauxRecus)
                .HasForeignKey(t => t.noMembreReceveur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Gift>()
                .HasRequired(p => p.emoticonEnvoye)
                .WithMany()
                .HasForeignKey(t => t.noEmoticonEnvoye);

            //Signalement
            modelBuilder.Entity<Signalement>()
                .HasRequired(m => m.membreFaisantPlainte)
                .WithMany(mem => mem.listeSignalementEnvoyer)
                .HasForeignKey(t => t.noMembreFaisantPlainte)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Signalement>()
                .HasRequired(m => m.membreContreQuiEstPlainte)
                .WithMany(mem => mem.listeSignalementContre)
                .HasForeignKey(t => t.noMembreContreQuiEstPlainte)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Signalement>() 
                .HasOptional(m => m.messageJoint)
                .WithMany()
                .HasForeignKey(t => t.noMessageJoint)
                .WillCascadeOnDelete(false);

            //Publications
            modelBuilder.Entity<Publication>()
                .HasRequired(p => p.envoyeur)
                .WithMany(mem => mem.listePublications)
                .HasForeignKey(t => t.noMembreEnvoyeur);

            //Avis Activité
            modelBuilder.Entity<AvisActivite>()
                .HasRequired(m => m.membreEnvoyeur)
                .WithMany()
                .HasForeignKey(t => t.noMembreEnvoyeur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AvisActivite>()
                .HasRequired(m => m.activiteAssocie)
                .WithMany(act => act.listeAvisActivite)
                .HasForeignKey(t => t.noActiviteAssocie)
                .WillCascadeOnDelete(false);

            //PhotosActivite
            modelBuilder.Entity<PhotosActivite>()
                .HasRequired(m => m.activite)
                .WithMany(act => act.listePhotosActivites)
                .HasForeignKey(t => t.noActivite)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PhotosActivite>()
                .HasRequired(m => m.membreQuiPublie)
                .WithMany()
                .HasForeignKey(t => t.noMembreQuiPublie)
                .WillCascadeOnDelete(false);

            //Signalement
            modelBuilder.Entity<Signalement>()
                .HasRequired(m => m.etatSignalementActuel)
                .WithMany()
                .HasForeignKey(t => t.noEtatSignalementActuel)
                .WillCascadeOnDelete(false);

            //Compte Admin
            modelBuilder.Entity<CompteAdmin>()
                .HasRequired(m => m.permission)
                .WithMany()
                .HasForeignKey(t => t.noPermission)
                .WillCascadeOnDelete(false);

            //Abonnement
            modelBuilder.Entity<Abonnement>()
                .HasRequired(a => a.provinceFacturation)
                .WithMany()
                .HasForeignKey(a => a.noProvince)
                .WillCascadeOnDelete(false);

            //Commentaire Signalement
            modelBuilder.Entity<CommentaireSignalement>()
                .HasRequired(a => a.compteAdminEnvoyeur)
                .WithMany()
                .HasForeignKey(a => a.noCompteAdminEnvoyeur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CommentaireSignalement>()
                .HasRequired(a => a.signalementLie)
                .WithMany(a => a.lesCommentairesSurCeSignalement)
                .HasForeignKey(a => a.noSignalementLie)
                .WillCascadeOnDelete(false);

            //TraitementSignalement
            modelBuilder.Entity<TraitementSignalement>()
                .HasOptional(a => a.messageLie)
                .WithMany()
                .HasForeignKey(a => a.noMessageLie)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TraitementSignalement>()
                .HasRequired(a => a.compteAdminTraiteur)
                .WithMany()
                .HasForeignKey(a => a.noCompteAdminTraiteur)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TraitementSignalement>()
                .HasRequired(a => a.signalementLie)
                .WithMany(a => a.lesTraitementsSurCeSignalement)
                .HasForeignKey(a => a.noSignalementLie)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TraitementSignalement>()
                .HasRequired(a => a.actionTraitement)
                .WithMany()
                .HasForeignKey(a => a.noActionTraitement)
                .WillCascadeOnDelete(false);
        }

        public static ClubContactContext Create()
        {
            return new ClubContactContext();
        }

        //Les membres dans la base de données
        public virtual DbSet<Membre> Membres { get; set; }      
        //Liste des photos dans la base de données
        public virtual DbSet<Photo> Photos { get; set; }        
        //Les activités dans la base de données
        public virtual DbSet<Activite> Activites { get; set; }  
        //Les messages dans la base de données
        public virtual DbSet<Message> Messages { get; set; }    
        //Les abonnements dans la base de données
        public virtual DbSet<Abonnement> Abonnements { get; set; }  
        //Les connexions d'un membre dans la base de données
        public virtual DbSet<Connexion> Connexions { get; set; }    
        //Les couleurs de cheveux possibles pour un membre dans la base de données
        public virtual DbSet<CheveuxCouleur> CheveuxCouleurs { get; set; } 
        //Les couleurs de yeux possibles pour un membre dans la base de données
        public virtual DbSet<YeuxCouleur> YeuxCouleurs { get; set; }       
        //Les hobbies dans la base de données
        public virtual DbSet<Hobbie> Hobbies { get; set; }      
        //Les origines possibles pour un membre dans la base de données
        public virtual DbSet<Origine> Origines { get; set; }    
        //Les provinces possibles pour un membre dans la base de données
        public virtual DbSet<Province> Provinces { get; set; }  
        //Les publications d'un membre dans la base de données
        public virtual DbSet<Publication> Publications { get; set; }    
        //Les raisons sur le site possibles pour un membre dans la base de données
        public virtual DbSet<RaisonsSurSite> RaisonsSurSites { get; set; }  
        //Les religions possibles pour un membre dans la base de données
        public virtual DbSet<Religion> Religions { get; set; }  
        //Les silhouettes possibles pour un membre dans la base de données
        public virtual DbSet<Silhouette> Silhouettes { get; set; }  
        //Les thèmes possibles pour le activités dans la base de données
        public virtual DbSet<Theme> Themes { get; set; }        
        //Les villes possibles pour les membres et les activités dans la base de données
        public virtual DbSet<Ville> Villes { get; set; }        
        //Les visites faites sur le profil d'un membre dans la base de données
        public virtual DbSet<Visite> Visites { get; set; }
        //Les tailles possibles pour les membres dans la base de données
        public virtual DbSet<Taille> Tailles { get; set; }    
        //Les types de hobbies possible
        public virtual DbSet<Types> Types { get; set;  }
        public virtual DbSet<Occupation> Occupations { get; set; }
        public virtual DbSet<SituationFinanciere> SituationsFinancieres { get; set; }
        public virtual DbSet<NiveauEtude> NiveauEtudes { get; set; }
        public virtual DbSet<DesirEnfant> DesirEnfants { get; set; }
        public virtual DbSet<Gift> Gifts { get; set; }
        public virtual DbSet<Emoticon> Emoticons { get; set; }

        public virtual DbSet<Signalement> Signalements { get; set; }

        public virtual DbSet<AvisActivite> AvisActivites { get; set; }
        public virtual DbSet<PhotosActivite> PhotosActivites { get; set; }

        //Gestion Administrative
        public virtual DbSet<NiveauDePermissionAdmin> NiveauDePermissionAdmins { get; set; }

        public virtual DbSet<CompteAdmin> CompteAdmins { get; set; }

        public virtual DbSet<EtatSignalement> EtatSignalements { get; set; }

        public virtual DbSet<ForfaitPremium> ForfaitPremiums { get; set; }

        //Signalement
        public virtual DbSet<TraitementSignalement> TraitementSignalements { get; set; }

        public virtual DbSet<CommentaireSignalement> CommentaireSignalements { get; set; }

        public virtual DbSet<ActionTraitement> ActionTraitements { get; set; }
    }
}