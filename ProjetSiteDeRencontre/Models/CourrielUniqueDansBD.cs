/*------------------------------------------------------------------------------------

CLASSE CONTENANT DES VALIDATION ATTRIBUTES CUSTOM:
    COURRIELUNIQUEDANSBD: S'ASSURE QU'IL N'Y AIT PAS 2 ADRESSES COURRIELS PAREIL DANS LA BASE DE DONNÉES
    NOMUNIQUEDANSBD: S'ASSURE QU'IL N'Y AIT PAS 2 COMPTES ADMINISTRATEURS AVEC LE MÊME NOM DANS LA BD

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CourrielUniqueDansBD : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ClubContactContext db = new ClubContactContext();

            try
            {
                Membre membreActuel = (Membre)validationContext.ObjectInstance;
                if (membreActuel == null) return new ValidationResult("Le model est vide");

                Membre membreAvecMemeCourriel = db.Membres.Where(m => m.courriel == value.ToString() && m.noMembre != membreActuel.noMembre &&
                                                                      !(m.compteSupprimeParAdmin == false)
                                                            ).FirstOrDefault();

                //Si aucun membre n'existe avec le même courriel
                if(membreAvecMemeCourriel == null)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Ce courriel est déjà utilisé. Veuillez en utiliser un autre.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Classe: UniqueDansBD, Type de classe : ValidationAttribute ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; Valeur de value: " + value.ToString(), e);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NomCompteAdminUniqueDansBD : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ClubContactContext db = new ClubContactContext();

            try
            {
                CompteAdmin compteActuel = (CompteAdmin)validationContext.ObjectInstance;
                if (compteActuel == null) return new ValidationResult("Le model est vide");

                CompteAdmin compteAvecMemeCourriel = null;
                if (value != null)
                {
                    compteAvecMemeCourriel = db.CompteAdmins.Where(m => m.nomCompte == value.ToString() && m.noCompteAdmin != compteActuel.noCompteAdmin).FirstOrDefault();
                }

                //Si aucun membre n'existe avec le même courriel
                if (compteAvecMemeCourriel == null)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Ce nom de compte Administrateur est déjà utilisé. Veuillez en utiliser un autre.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Classe: NomCompteAdminUniqueDansBD, Type de classe : ValidationAttribute ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; Valeur de value: " + value.ToString(), e);
            }
        }
    }
}