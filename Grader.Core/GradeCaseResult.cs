﻿using System.Collections.Generic;
using System.Linq;

namespace Grader
{
    public class GradeCaseResult : IGradeCaseResult
    {
        public GradeCaseResult(IGradeCase @case, IEnumerable<string> actualOutput, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage) != true)
            {
                _hasErrors = true;
                ErrorMessage = $"Case {@case.CaseNumber}: " + errorMessage;
            }
            Case = @case;
            ActualOutput = actualOutput;
            Evaluate();
        }

        private bool _hasErrors;
        private void Evaluate()
        {
            Pass = true;
            if(_hasErrors)
            {
                Pass = false;
                return;
            }
            foreach (var caseExpectedOutput in Case.ExpectedOutputs)
            {
                if (ActualOutput.All(p => p?.Contains(caseExpectedOutput) != true))
                {
                    Pass = false;
                }
            }

            if (!Pass)
            {
                ErrorMessage = $"Case {Case.CaseNumber}: Failed";
            }
        }

        public IGradeCase Case { get; }
        public IEnumerable<string> ActualOutput { get; private set; }
        public bool Pass { get; private set; }
        public string ErrorMessage { get; set; }
    }
}