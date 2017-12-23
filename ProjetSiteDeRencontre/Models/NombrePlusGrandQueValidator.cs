/*------------------------------------------------------------------------------------

VALIDATEUR CUSTOM QUI VÉRIFIE SI UN CHIFFRE EST PLUS PETIT OU PLUS GRAND QU'UN AUTRE CHIFFRE,
ET EST UTILISÉ COMME DATA ANNOTATION PERSONNALISÉ

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
using ProjetSiteDeRencontre.LesUtilitaires;

namespace ProjetSiteDeRencontre.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NombrePlusGrandQue : ValidationAttribute, IClientValidatable
    {
        string otherPropertyName;

        public NombrePlusGrandQue(string otherPropertyName, string errorMessage) : base(errorMessage)
        {
            this.otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            try
            {
                //Avec le contexte on peut retrouver une référence vers la valeur de l'autre propriété à utiliser pour faire la validation
                var otherPropertyInfo = validationContext.ObjectType.GetProperty(this.otherPropertyName);

                if (otherPropertyInfo.PropertyType.Equals(typeof(int?)))
                {
                    int? toValidate = (int?)value;
                    int? referenceProperty = (int?)otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

                    if (toValidate < referenceProperty)
                    {
                        validationResult = new ValidationResult(ErrorMessageString);
                    }
                }
                else
                {
                    validationResult = new ValidationResult("Erreur lors de la validation de la propriété NombrePlusGrandQue. OtherProperty n'est pas nullable<int>.");
                }
            }
            catch (Exception e)
            {
                Utilitaires.templateException("IsValid", "NombrePlusGrandQueValidator", "", e);
            }

            return validationResult;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string errorMessage = ErrorMessageString;

            ModelClientValidationRule nombrePlusGrandQueValidator = new ModelClientValidationRule();
            nombrePlusGrandQueValidator.ErrorMessage = errorMessage;
            nombrePlusGrandQueValidator.ValidationType = "nombreplusgrandque";
            nombrePlusGrandQueValidator.ValidationParameters.Add("otherpropertyname", otherPropertyName);

            yield return nombrePlusGrandQueValidator;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class NombrePlusPetitQue : ValidationAttribute, IClientValidatable
    {
        string otherPropertyName;

        public NombrePlusPetitQue(string otherPropertyName, string errorMessage) : base(errorMessage)
        {
            this.otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            try
            {
                //Avec le contexte on peut retrouver une référence vers la valeur de l'autre propriété à utiliser pour faire la validation
                var otherPropertyInfo = validationContext.ObjectType.GetProperty(this.otherPropertyName);

                if (otherPropertyInfo.PropertyType.Equals(typeof(int?)))
                {
                    int? toValidate = (int?)value;
                    int? referenceProperty = (int?)otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

                    if (toValidate > referenceProperty)
                    {
                        validationResult = new ValidationResult(ErrorMessageString);
                    }
                }
                else
                {
                    validationResult = new ValidationResult("Erreur lors de la validation de la propriété NombrePlusPetitQue. OtherProperty n'est pas nullable<int>.");
                }
            }
            catch (Exception e)
            {
                throw Utilitaires.templateException("IsValid", "NombrePlusPetitQueValidator", "", e);
            }

            return validationResult;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string errorMessage = ErrorMessageString;

            ModelClientValidationRule NombrePlusPetitQueValidator = new ModelClientValidationRule();
            NombrePlusPetitQueValidator.ErrorMessage = errorMessage;
            NombrePlusPetitQueValidator.ValidationType = "nombrepluspetitque";
            NombrePlusPetitQueValidator.ValidationParameters.Add("otherpropertyname", otherPropertyName);

            yield return NombrePlusPetitQueValidator;
        }
    }
}