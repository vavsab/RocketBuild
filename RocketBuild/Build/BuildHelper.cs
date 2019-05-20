﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace RocketBuild.Build
{
    public class BuildHelper
    {
        public async Task<List<DisplayBuild>> GetBuildsAsync()
        {
            var client = GetClient<BuildHttpClient>();
            var tfsClient = GetClient<TfvcHttpClient>();

            List<BuildDefinitionReference> definitions = await client.GetDefinitionsAsync(Settings.Current.Project, name: null);
            var tfsBuilds = await client.GetBuildsAsync(Settings.Current.Project, definitions: definitions.Select(d => d.Id), maxBuildsPerDefinition: 1, queryOrder: BuildQueryOrder.FinishTimeDescending);

            var builds = new List<DisplayBuild>();

            TfvcChangesetRef changeset = (await tfsClient.GetChangesetsAsync(Settings.Current.Project, top: 1)).FirstOrDefault();

            foreach (BuildDefinitionReference definition in definitions)
            {
                var lastTfsBuild = tfsBuilds.FirstOrDefault(b => b.Definition.Id == definition.Id);
                string lastTfsBuildLink = (lastTfsBuild?.Links.Links["web"] as ReferenceLink)?.Href;

                builds.Add(new DisplayBuild
                {
                    DefinitionId = definition.Id,
                    Name = definition.Name,
                    LastBuild = lastTfsBuild != null ? Regex.Match(lastTfsBuild.BuildNumber, Settings.Current.BuildNameExtractVersionRegex).Value : String.Empty,
                    LastBuildStatus = (DisplayBuildStatus?)lastTfsBuild?.Status,
                    LastBuildResult = (BuildResult?)lastTfsBuild?.Result,
                    LastBuildLink = lastTfsBuildLink,
                    LastCheckin = changeset?.ChangesetId.ToString()
                });
            }

            return builds;
        }

        public async Task StartBuildAsync(DisplayBuild build)
        {
            var client = GetClient<BuildHttpClient>();

            await client.QueueBuildAsync(new Microsoft.TeamFoundation.Build.WebApi.Build
            {
                Definition = new DefinitionReference { Id = build.DefinitionId }
            }, Settings.Current.Project);
        }

        private static T GetClient<T>()
            where T : VssHttpClientBase
        {
            var accountUri = new Uri(Settings.Current.AccountUrl);
            var connection = new VssConnection(accountUri, new VssBasicCredential(null, Settings.Current.ApiKey));

            return connection.GetClient<T>();
        }
    }
}
