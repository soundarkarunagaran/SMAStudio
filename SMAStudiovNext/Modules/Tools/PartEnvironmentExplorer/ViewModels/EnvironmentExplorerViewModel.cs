﻿using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;
using Gemini.Modules.Output;
using SMAStudiovNext.Commands;
using SMAStudiovNext.Core;
using SMAStudiovNext.Modules.DialogConnectionManager.Windows;
using SMAStudiovNext.Modules.PartEnvironmentExplorer.Commands;
using SMAStudiovNext.Modules.PartEnvironmentExplorer.Views;
using SMAStudiovNext.Utils;
using System.Windows.Controls;
using System;
using SMAStudiovNext.Models;
using System.Windows;

namespace SMAStudiovNext.Modules.PartEnvironmentExplorer.ViewModels
{
    [Export(typeof(EnvironmentExplorerViewModel))]
    public class EnvironmentExplorerViewModel : Tool, ICommandHandler<NewConnectionCommandDefinition>
    {
        private readonly ObservableCollection<ResourceContainer> _items;
        private readonly ICommand _publishCommand;
        private IEnvironmentExplorerView _view;

        #region ITool Properties
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        } 

        public ObservableCollection<ResourceContainer> Items
        {
            get { return _items; }
        }
        
        /*/// <summary>
        /// Declare new instead of override since we want to add a set method as well
        /// </summary>
        public new double PreferredWidth
        {
            get; set;
        }

        /// <summary>
        /// Declare new instead of override since we want to add a set method as well
        /// </summary>
        public new double PreferredHeight
        {
            get; set;
        }*/

        public override string DisplayName
        {
            get { return "Environment Explorer"; }
        }
        #endregion

        public EnvironmentExplorerViewModel()
        {
            _items = new ObservableCollection<ResourceContainer>();
            _publishCommand = new PublishCommand();
        }

        protected override void OnViewLoaded(object view)
        {
            _view = (IEnvironmentExplorerView)view;
        }

        public IBackendContext GetCurrentContext()
        {
            return _view.GetCurrentContext();
        }

        public void OnBackendReady(object sender, ContextUpdatedEventArgs e)
        {
            var context = e.Context;
            
            Execute.OnUIThread(() =>
            {
                var output = IoC.Get<IOutput>();
                output.AppendLine("All objects loaded!");

                NotifyOfPropertyChange(() => Items);

                // Add each account to the 'Copy To' button in the context menu
                if (_view != null)
                {
                    if (_view.CopyButton.Items.Count == 0)
                    {
                        foreach (var item in Items)
                        {
                            var menuItem = new MenuItem();
                            menuItem.Header = item.Title;
                            menuItem.Tag = item;
                            menuItem.Click += CopyToClicked;

                            _view.CopyButton.Items.Add(menuItem);
                        }
                    }
                }
            });
        }

        private async void CopyToClicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem == null || menuItem.Tag == null)
                return;

            var selectedItem = _view.SelectedObject;

            if (selectedItem == null)
                return;

            var tag = selectedItem.Tag;
            var copyToService = ((menuItem.Tag as ResourceContainer).Tag as BackendContext);

            LongRunningOperation.Start();

            if (tag is RunbookModelProxy)
            {
                var runbook = (tag as RunbookModelProxy);
                var result = await copyToService.Copy(runbook);

                if (!result)
                {
                    MessageBox.Show("You can only copy the resource between different accounts.", "Error");
                }

                LongRunningOperation.Stop();
            }
            else
                MessageBox.Show("Sorry, currently only runbooks are supported in the copy feature.", "Currently in work");
        }

        public void Delete(ResourceContainer item)
        {
            Execute.OnUIThread(() =>
            {
                // This is kinda tricky since we have multiple levels of nested items
                var parentNode = Items.TreeFind(null, item);

                if (parentNode != null)
                    parentNode.Items.Remove(item);

                NotifyOfPropertyChange(() => Items);
            });
        }

        void ICommandHandler<NewConnectionCommandDefinition>.Update(Command command)
        {
            // Ignore
        }

        Task ICommandHandler<NewConnectionCommandDefinition>.Run(Command command)
        {
            var connManagerWindow = new ConnectionManagerWindow();
            connManagerWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            connManagerWindow.ShowDialog();

            return TaskUtility.Completed;
        }

        public ICommand LoadCommand
        {
            get { return AppContext.Resolve<ICommand>("LoadCommand"); }
        }

        public ICommand HistoryCommand
        {
            get { return AppContext.Resolve<ICommand>("HistoryCommand"); }
        }

        public ICommand DeleteCommand
        {
            get { return AppContext.Resolve<ICommand>("DeleteCommand"); }
        }

        public ICommand PublishCommand
        {
            get {
                return _publishCommand;
            }
        }

        public ICommand NewCredentialCommand
        {
            get { return AppContext.Resolve<ICommand>("NewCredentialCommand"); }
        }

        public ICommand NewVariableCommand
        {
            get { return AppContext.Resolve<ICommand>("NewVariableCommand"); }
        }

        public ICommand NewScheduleCommand
        {
            get { return AppContext.Resolve<ICommand>("NewScheduleCommand"); }
        }

        public ICommand NewRunbookCommand
        {
            get { return AppContext.Resolve<ICommand>("NewRunbookCommand"); }
        }

        public ICommand NewModuleCommand
        {
            get { return AppContext.Resolve<ICommand>("NewModuleCommand"); }
        }

        public ICommand NewConnectionObjectCommand
        {
            get { return AppContext.Resolve<ICommand>("NewConnectionObjectCommand"); }
        }

        public ICommand NewConnectionCommand
        {
            get { return AppContext.Resolve<ICommand>("NewConnectionCommand"); }
        }

        public ICommand RefreshCommand
        {
            get { return AppContext.Resolve<ICommand>("RefreshCommand"); }
        }

        public ICommand DocumentationCommand
        {
            get { return AppContext.Resolve<ICommand>("DocumentationCommand"); }
        }
    }
}
