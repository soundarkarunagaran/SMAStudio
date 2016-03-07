﻿using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;

namespace SMAStudiovNext.Modules.Runbook.Editor.Completion
{
    public class CompletionResult
    {
        public CompletionResult(IList<ICompletionData> completionData)
        {
            CompletionData = completionData;
        }

        public IList<ICompletionData> CompletionData { get; private set; }
    }
}