﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using RocketBuild.Helpers;

namespace RocketBuild.Deploy
{
    public class DeployHelper
    {
        public async Task<List<DisplayEnvironment>> GetEnvironmentsAsync()
        {
            var client = VssClientHelper.GetClient<ReleaseHttpClient>(
                Settings.GlobalSettings.Current.AccountUrl,
                Settings.GlobalSettings.Current.ApiKey,
                Settings.GlobalSettings.Current.UseSsl);
            var environments = new List<DisplayEnvironment>();

            List<ReleaseDefinition> definitions = await client.GetReleaseDefinitionsAsync(Settings.GlobalSettings.Current.Project, expand: ReleaseDefinitionExpands.Environments);
            string[] environmentNames = definitions.SelectMany(d => d.Environments).Select(e => e.Name).Distinct().ToArray();

            List<Release> releases = await client.GetReleasesAsync(
                Settings.GlobalSettings.Current.Project, 
                expand: ReleaseExpands.Environments, 
                // Reduce number of releases to load faster
                minCreatedTime: DateTime.Today.AddMonths(-6));

            foreach (string environmentName in environmentNames)
            {
                var environment = new DisplayEnvironment
                {
                    Name = environmentName
                };

                foreach (ReleaseDefinition definition in definitions)
                {
                    ReleaseDefinitionEnvironment definitionEnvironment = definition.Environments.FirstOrDefault(e => e.Name == environmentName);
                    if (definitionEnvironment == null)
                        continue;

                    Release[] definitionReleases = releases
                        .Where(r => r.ReleaseDefinitionReference.Id == definition.Id && r.Environments.Any(e => e.Name == environmentName))
                        .OrderBy(r => r.CreatedOn)
                        .ToArray();

                    Release lastSuccess = definitionReleases.LastOrDefault(r => r.Environments.First(e => e.Name == environmentName).Status == EnvironmentStatus.Succeeded);
                    Release last = definitionReleases.LastOrDefault();

                    ReleaseEnvironment releaseEnvironment = last?.Environments.First(e => e.Name == environmentName);

                    environment.Releases.Add(new DisplayRelease
                    {
                        Id = last?.Id ?? 0,
                        DefinitionId = definition.Id,
                        EnvironmentId = releaseEnvironment?.Id,
                        Name = definition.Name,
                        AvailableVersion = last != null ? Regex.Match(last.Name, Settings.GlobalSettings.Current.ReleaseNameExtractVersionRegex).Value : null,
                        AvailableVersionLink = (last?.Links.Links["web"] as ReferenceLink)?.Href,
                        LastDeployedVersion = lastSuccess != null ? Regex.Match(lastSuccess.Name, Settings.GlobalSettings.Current.ReleaseNameExtractVersionRegex).Value : String.Empty,
                        LastDeployedVersionLink = (lastSuccess?.Links.Links["web"] as ReferenceLink)?.Href,
                        Status = (DisplayReleaseStatus?)releaseEnvironment?.Status
                    });
                }

                environments.Add(environment);
            }

            return environments;
        }

        public async Task StartDeploymentAsync(DisplayRelease release)
        {
            if (!release.EnvironmentId.HasValue)
            {
                MessageBox.Show($"Release definition '{release.Name}' has no any release. Setup automatic release creation in TFS or create release manually.");
                return;
            }

            var client = VssClientHelper.GetClient<ReleaseHttpClient>(
                Settings.GlobalSettings.Current.AccountUrl,
                Settings.GlobalSettings.Current.ApiKey,
                Settings.GlobalSettings.Current.UseSsl);

            await client.UpdateReleaseEnvironmentAsync(new ReleaseEnvironmentUpdateMetadata
            {
                Status = EnvironmentStatus.InProgress,
                Comment = "Started from RocketBuild application"
            }, Settings.GlobalSettings.Current.Project, release.Id, release.EnvironmentId.Value);
        }
    }
}
