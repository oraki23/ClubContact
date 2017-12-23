/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ DANS LA CONNEXION

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Classe permettant la connection en gardant le login et le password de la vue login
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Veuillez entrer votre courriel")]
    [Display(Name = "Ton courriel :")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Veuillez enter votre mot de passe")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe :")]
    public string Password { get; set; }

    [DisplayName("Se souvenir de moi?")]
    public bool seSouvenirDeMoi { get; set; }
}