using System;
using AutoMapper;
using CodeCampServer.Core;
using CodeCampServer.Infrastructure.CommandProcessor.CommandConfiguration;
using CommandProcessor;
using Microsoft.Practices.ServiceLocation;
using Tarantino.RulesEngine.CommandProcessor;

namespace CodeCampServer.Infrastructure.CommandProcessor
{
	public class RulesEngineConfiguration : IRequiresConfigurationOnStartup
	{
		public static void Configure(Type typeToLocatorConfigurationAssembly)
		{
			var rulesEngine = new global::CommandProcessor.RulesEngine();
			global::CommandProcessor.RulesEngine.MessageProcessorFactory = new MessageProcessorFactory();
			rulesEngine.Initialize(typeToLocatorConfigurationAssembly.Assembly, new CcsMessageMapper());
		}

		public class CcsMessageMapper : IMessageMapper
		{
			public object MapUiMessageToCommandMessage(object message, Type messageType, Type destinationType)
			{
				return Mapper.Map(message, message.GetType(), destinationType);
			}
		}

		public void Configure()
		{
			Configure(typeof (DeleteMeetingMessageConfiguration));
		}
	}
}