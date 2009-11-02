using CodeCampServer.Core.Domain.Model;
using Tarantino.RulesEngine.CommandProcessor;

namespace CodeCampServer.Core.Services.BusinessRule.UpdateUserGroup
{
	public class UpdateUserGroupCommandMessage : ICommandMessage
	{
		public UserGroup UserGroup { get; set; }
	}
}