﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using RocketBuild.Build;
using RocketBuild.Deploy;
using RocketBuild.Extensions;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RocketBuild
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DeployHelper deployHelper = new DeployHelper();
        private readonly BuildHelper buildHelper = new BuildHelper();

        public MainViewModel()
        {
            RefreshBuilds()
                .ContinueWith(t => RefreshEnvironments()
                    .ContinueWith(t2 => RestoreSelections()));
        }

        public bool IsBusy
        {
            get => GetProperty(() => IsBusy);
            set => SetProperty(() => IsBusy, value);
        }

        public List<DisplayBuild> Builds
        {
            get => GetProperty(() => Builds);
            set => SetProperty(() => Builds, value);
        }

        public List<DisplayEnvironment> Environments
        {
            get => GetProperty(() => Environments);
            set => SetProperty(() => Environments, value);
        }

        public DisplayEnvironment SelectedEnvironment
        {
            get => GetProperty(() => SelectedEnvironment);
            set
            {
                SetProperty(() => SelectedEnvironment, value);
                CurrentReleases = value?.Releases
                    .OrderBy(r => r.Name)
                    .ToList();
            }
        }

        public List<DisplayRelease> CurrentReleases
        {
            get => GetProperty(() => CurrentReleases);
            set => SetProperty(() => CurrentReleases, value);
        }

        public void OnClosing()
        {
            Settings.Current.LastSelectedEnvironment = SelectedEnvironment?.Name;

            Settings.Current.LastSelectedBuildIds.Clear();
            Settings.Current.LastSelectedBuildIds.AddRange(Builds
                    ?.Where(b => b.IsChecked)
                    .Select(b => b.DefinitionId));

            Settings.Current.LastSelectedReleaseIds.Clear();
            Settings.Current.LastSelectedReleaseIds.AddRange(Environments
                    .ToDictionary(e => e.Name, e => e.Releases
                        .Where(r => r.IsChecked)
                        .Select(r => r.Id)));

            Settings.Save();
        }

        [Command]
        public async Task RefreshBuilds()
        {
            try
            {
                IsBusy = true;

                int[] backupSelectedBuildIds = Builds
                    ?.Where(b => b.IsChecked)
                    .Select(b => b.DefinitionId)
                    .ToArray();

                Builds = (await buildHelper.GetBuildsAsync())
                    .OrderBy(b => b.Name)
                    .ToList();

                if (backupSelectedBuildIds != null && Builds != null)
                {
                    foreach (DisplayBuild build in Builds)
                    {
                        build.IsChecked = backupSelectedBuildIds.Contains(build.DefinitionId);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception > {nameof(RefreshBuilds)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [Command]
        public async Task RefreshEnvironments()
        {
            try
            {
                IsBusy = true;

                string backupSelectedEnvironmentName = SelectedEnvironment?.Name;

                int[] backupSelectedReleases = CurrentReleases
                    ?.Where(r => r.IsChecked)
                    .Select(r => r.Id)
                    .ToArray();

                Environments = (await deployHelper.GetEnvironmentsAsync())
                    .OrderBy(e => e.Name)
                    .ToList();

                if (backupSelectedEnvironmentName != null)
                {
                    SelectedEnvironment = Environments
                        .FirstOrDefault(e => String.Equals(e.Name, backupSelectedEnvironmentName,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (backupSelectedReleases != null && CurrentReleases != null)
                {
                    foreach (DisplayRelease release in CurrentReleases)
                    {
                        release.IsChecked = backupSelectedReleases.Contains(release.Id);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception > {nameof(RefreshEnvironments)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [Command]
        public async void QueueBuilds()
        {
            try
            {
                if (Builds?.Any(b => b.IsChecked) != true)
                {
                    MessageBox.Show("Nothing to build.");
                    return;
                }

                DisplayBuild[] buildsToQueue = Builds
                    .Where(b => b.IsChecked)
                    .ToArray();

                if (MessageBox.Show(
                        $"Do you want to queue builds:\n\n{String.Join("\n", buildsToQueue.Select(b => b.Name))}",
                        "Confirm build", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2)
                    != DialogResult.Yes)
                    return;

                IsBusy = true;

                foreach (DisplayBuild build in buildsToQueue)
                {
                    await buildHelper.StartBuildAsync(build);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception > {nameof(QueueBuilds)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [Command]
        public async void QueueDeploy()
        {
            try
            {
                if (CurrentReleases?.Any(b => b.IsChecked) != true)
                {
                    MessageBox.Show("Nothing to deploy.");
                    return;
                }

                DisplayRelease[] releasesToQueue = CurrentReleases
                    .Where(r => r.IsChecked)
                    .ToArray();

                if (MessageBox.Show(
                        $"Do you want to deploy releases:\n\n{String.Join("\n", releasesToQueue.Select(b => b.Name))}",
                        "Confirm deploy", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2)
                    != DialogResult.Yes)
                    return;

                IsBusy = true;

                foreach (DisplayRelease release in releasesToQueue)
                {
                    await deployHelper.StartDeploymentAsync(release);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception > {nameof(QueueDeploy)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #region Title commands

        [Command]
        public void MinimizeWindow(Window window) => SystemCommands.MinimizeWindow(window);

        [Command]
        public void MaximizeWindow(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(window);
            else
                SystemCommands.MaximizeWindow(window);
        }

        [Command]
        public void CloseWindow(Window window) => SystemCommands.CloseWindow(window);

        #endregion

        private void RestoreSelections()
        {
            foreach (DisplayBuild build in Builds)
            {
                build.IsChecked = Settings.Current.LastSelectedBuildIds.Contains(build.DefinitionId);
            }

            foreach (DisplayEnvironment environment in Environments)
            {
                int[] lastSelectedReleases = Settings.Current.LastSelectedReleaseIds
                    .GetValueOrDefault(environment.Name)
                    .ToArray();

                foreach (DisplayRelease release in environment.Releases)
                {
                    release.IsChecked = lastSelectedReleases.Contains(release.Id);
                }
            }

            SelectedEnvironment = Environments
                .FirstOrDefault(e => String.Equals(e.Name, Settings.Current.LastSelectedEnvironment,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
