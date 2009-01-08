using System;
using CodeCampServer.UI.Models.Forms.Attributes;
using CodeCampServer.UI.Models.Validation.Attributes;
using Castle.Components.Validator;

namespace CodeCampServer.UI.Models.Forms
{
    public class UserForm
    {
        [BetterValidateNonEmpty("Username")]
        [ValidateRegExp(@"^([a-zA-Z])[a-zA-Z_-]*[\w_-]*[\S]$|^([a-zA-Z])[0-9_-]*[\S]$|^[a-zA-Z]*[\S]$","Username is not valid.")]
        public string Name { get; set; }

        [BetterValidateNonEmpty("Email")]
        [BetterValidateEmail("Email")]
        public string EmailAddress { get; set; }

        [Hidden]
        public Guid Id { get; set; }

        [BetterValidateNonEmpty("Password")]        
        public string Password { get; set; }
    }
}