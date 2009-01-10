using System.Web.Mvc.Html;
using CodeCampServer.Core;

namespace CodeCampServer.UI.Helpers.ViewPage.InputBuilders
{
	public class TextBoxInputBuilder : BaseInputBuilder
	{
		public override bool IsSatisfiedBy(IInputSpecification specification)
		{
			return specification.PropertyInfo.PropertyType == typeof (string);
		}

		protected override string CreateInputElementBase()
		{
			object customAttributes = InputSpecification.CustomAttributes;
			if (customAttributes != null && customAttributes.ToDictionary().ContainsKey("rows"))
			{
				return InputSpecification.Helper.TextArea(InputSpecification.InputName, GetValue().ToNullSafeString(),
				                                          customAttributes);
			}

			return InputSpecification.Helper.TextBox(InputSpecification.InputName, GetValue(), customAttributes);
		}
	}
}