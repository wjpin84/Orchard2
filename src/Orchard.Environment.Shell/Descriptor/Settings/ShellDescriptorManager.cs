using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using YesSql.Core.Services;

namespace Orchard.Environment.Shell.Descriptor.Settings
{
    public class ShellDescriptorManager : Component, IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;

        public ShellDescriptorManager(
            ShellSettings shellSettings,
            IEventBus eventBus,
            ILogger<ShellDescriptorManager> logger,
            ISession session) : base(logger)
        {
            _shellSettings = shellSettings;
            _eventBus = eventBus;
            _session = session;
        }

        public ShellDescriptor GetShellDescriptor()
        {
            return _session.QueryAsync<ShellDescriptor>().FirstOrDefault().Result;
        }

        public async void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            var shellDescriptorRecord = GetShellDescriptor();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell descriptor").ToString());

            if (Logger.IsEnabled(LogLevel.Information))
            {
            	Logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);
            }
            if (shellDescriptorRecord == null)
            {
                shellDescriptorRecord = new ShellDescriptor { SerialNumber = 1 };
                _session.Save(shellDescriptorRecord);
            }
            else
            {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features.Clear();
            foreach (var feature in enabledFeatures)
            {
                shellDescriptorRecord.Features.Add(new ShellFeature { Name = feature.Name });
            }
            if (Logger.IsEnabled(LogLevel.Debug))
            {
            	Logger.LogDebug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", enabledFeatures.Select(feature => feature.Name)));
            }

            shellDescriptorRecord.Parameters.Clear();
            foreach (var parameter in parameters)
            {
                shellDescriptorRecord.Parameters.Add(new ShellParameter
                {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value
                });
            }
            if (Logger.IsEnabled(LogLevel.Debug))
            {
            	Logger.LogDebug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));
            }
            if (Logger.IsEnabled(LogLevel.Information))
            {
            	Logger.LogInformation("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);
            }
			
            await _eventBus.NotifyAsync<IShellDescriptorManagerEventHandler>(
                  e => e.Changed(shellDescriptorRecord, _shellSettings.Name));
        }
    }
}