﻿using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.Output;
using SMAStudiovNext.Core;
using SMAStudiovNext.Models;
using SMAStudiovNext.Modules.Runbook.ViewModels;
using SMAStudiovNext.Services;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace SMAStudiovNext.Agents
{
    public class AutoSaveAgent : IAgent, IDisposable
    {
        private readonly IOutput _output;
        private readonly IShell _shell;
        private readonly IModule _application;

        private object _syncLock = new object();
        private bool _isRunning = true;

        public AutoSaveAgent()
        {
            _output = IoC.Get<IOutput>();
            _shell = IoC.Get<IShell>();
            _application = IoC.Get<IModule>();
        }

        public void Start()
        {
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);

            var files = Directory.GetFiles(CacheFolder);

            if (files.Length == 0)
                return;

            _output.AppendLine("Found " + files.Length + " objects to recover.");

            var result = MessageBox.Show("Do you want to restore recovered objects?\r\nIf no, the recovered objects will be forgotten.", "Restore objects?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Rewrite the auto save agent to be context aware
                /*var context = ((SmaService)_smaService).GetConnection();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var contentReader = new StreamReader(file);
                    var objectContent = contentReader.ReadToEnd();

                    contentReader.Close();

                    AsyncExecution.Run(ThreadPriority.Normal, delegate ()
                    {
                        try
                        {
                            var runbook = context.Runbooks.FirstOrDefault(x => x.RunbookID.ToString().Equals(fileInfo.Name));

                            if (runbook == null)
                            {
                                _output.AppendLine("No runbook was found with ID '" + fileInfo.Name + "'.");
                                return;
                            }

                            if (!runbook.DraftRunbookVersionID.HasValue && MessageBox.Show(runbook.RunbookName + " is currently not checked out, do you want to check out the runbook?", "Published Runbook", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                return;
                            }

                            //var runbookViewModel = new RunbookViewModel(new RunbookModelProxy(runbook));
                            MessageBox.Show("Reimplement this with support for both Azure and SMA!");
                            //await runbookViewModel.CheckOut();

                            //runbookViewModel.Content = objectContent;

                            //_shell.OpenDocument(runbookViewModel);
                        }
                        catch (DataServiceQueryException ex)
                        {
                            _output.AppendLine("Error when retrieving runbook from SMA: " + ex.Message);
                        }
                    });
                }*/
            }
            else
            {
                foreach (var file in files)
                    File.Delete(file);
            }

            var backgroundThread = new Thread(new ThreadStart(StartInternal));
            backgroundThread.Priority = ThreadPriority.BelowNormal;
            backgroundThread.Start();
        }

        private void StartInternal()
        {
            while (_isRunning)
            {
                IList<IDocument> documents = null;

                lock (_syncLock)
                {
                    documents = _shell.Documents;
                }

                foreach (var document in documents)
                {
                    if (!_isRunning)
                        break;

                    if (!(document is RunbookViewModel))
                        continue;

                    var runbookViewModel = (RunbookViewModel)document;

                    if (!runbookViewModel.UnsavedChanges)
                        continue;

                    try
                    {
                        var textWriter = new StreamWriter(Path.Combine(AppHelper.CachePath, "cache", runbookViewModel.Runbook.RunbookID.ToString()), false);
                        textWriter.Write(runbookViewModel.Content);
                        textWriter.Flush();
                        textWriter.Close();
                    }
                    catch (IOException)
                    {
                        
                    }
                }

                Thread.Sleep(10 * 1000);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private string CacheFolder
        {
            get
            {
                return Path.Combine(AppHelper.CachePath, "cache");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _isRunning = false;

                    // If we close the application in a controlled way, we delete the
                    // cached files since these are onyl used in recovery scenarios.
                    var files = Directory.GetFiles(Path.Combine(AppHelper.CachePath, "cache"));

                    foreach (var file in files)
                        File.Delete(file);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AutoSaveAgent() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}