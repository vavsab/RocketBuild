using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using RocketBuild.Build;
using RocketBuild.Deploy;
using RocketBuild.Extensions;
using MessageBox = System.Windows.Forms.MessageBox;

// ReSharper disable LocalizableElement

namespace RocketBuild
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DeployHelper deployHelper = new DeployHelper();
        private readonly BuildHelper buildHelper = new BuildHelper();

        private List<DisplayBuild> builds;
        private List<DisplayEnvironment> environments;
        private DisplayEnvironment selectedEnvironment;
        private List<DisplayRelease> currentReleases;
        private bool isBusy;

        public MainViewModel()
        {
            RefreshBuildsCommand = new RelayCommand(async () => await OnRefreshBuildsCommand());
            RefreshEnvironmentsCommand = new RelayCommand(async () => await OnRefreshEnvironmentsCommand());
            QueueBuildsCommand = new RelayCommand(OnQueueBuildsCommand);
            QueueDeployCommand = new RelayCommand(OnQueueDeployCommand);

            OnRefreshBuildsCommand()
                .ContinueWith(t => OnRefreshEnvironmentsCommand()
                    .ContinueWith(t2 => RestoreSelections()));
        }

        public RelayCommand RefreshBuildsCommand { get; }

        public RelayCommand RefreshEnvironmentsCommand { get; }

        public RelayCommand QueueBuildsCommand { get; }

        public RelayCommand QueueDeployCommand { get; }

        #region Title buttons

        public RelayCommand<Window> MinimizeWindowCommand { get; } = new RelayCommand<Window>(MinimizeWindow);

        public RelayCommand<Window> MaximizeWindowCommand { get; } = new RelayCommand<Window>(MaximizeWindow);

        public RelayCommand<Window> CloseWindowCommand { get; } = new RelayCommand<Window>(CloseWindow);

        private static void MinimizeWindow(Window window) => SystemCommands.MinimizeWindow(window);

        private static void MaximizeWindow(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(window);
            else
                SystemCommands.MaximizeWindow(window);
        }

        private static void CloseWindow(Window window) => SystemCommands.CloseWindow(window);

        #endregion

        public bool IsBusy
        {
            get => isBusy;
            set => Set(ref isBusy, value);
        }

        public List<DisplayBuild> Builds
        {
            get => builds;
            set => Set(ref builds, value);
        }

        public List<DisplayEnvironment> Environments
        {
            get => environments;
            set => Set(ref environments, value);
        }

        public DisplayEnvironment SelectedEnvironment
        {
            get => selectedEnvironment;
            set
            {
                Set(ref selectedEnvironment, value);
                CurrentReleases = selectedEnvironment?.Releases
                    .OrderBy(r => r.Name)
                    .ToList();
            }
        }

        public List<DisplayRelease> CurrentReleases
        {
            get => currentReleases;
            set => Set(ref currentReleases, value);
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

        private async Task OnRefreshBuildsCommand()
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
                MessageBox.Show($"Exception > {nameof(OnRefreshBuildsCommand)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnRefreshEnvironmentsCommand()
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
                MessageBox.Show($"Exception > {nameof(OnRefreshEnvironmentsCommand)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

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

        private async void OnQueueBuildsCommand()
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
                MessageBox.Show($"Exception > {nameof(OnQueueBuildsCommand)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnQueueDeployCommand()
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
                MessageBox.Show($"Exception > {nameof(OnQueueDeployCommand)}: {e}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
