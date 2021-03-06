﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMAStudiovNext.Core;
using SMAStudiovNext.Utils;

namespace SMAStudiovNext.Modules.PartEnvironmentExplorer.Views
{
    /// <summary>
    /// Interaction logic for RunbookExplorer.xaml
    /// </summary>
    public partial class EnvironmentExplorerView : UserControl, IEnvironmentExplorerView
    {
        public EnvironmentExplorerView()
        {
            InitializeComponent();
        }

        public void RefreshSource()
        {
            treeView.Items.Refresh();
        }

        public IBackendContext GetCurrentContext()
        {
            return ((ResourceContainer)treeView.SelectedItem).Context;
        }

        public ResourceContainer SelectedObject
        {
            get { return (ResourceContainer)treeView.SelectedItem; }
        }

        public MenuItem CopyButton
        {
            get { return btnCopyResource; }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            treeView.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
        }

        private void OnPreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
                treeViewItem.Focus();
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
    }
}
