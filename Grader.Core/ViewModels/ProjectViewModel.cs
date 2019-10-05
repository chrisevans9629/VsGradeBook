﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncToolWindowSample.Models;
using Grader;
using Console = System.Console;

namespace AsyncToolWindowSample.ToolWindows
{
    public class ProjectViewModel : BindableViewModel
    {
        private readonly IVisualStudioService _visualStudioService;
        private readonly IConsoleAppGrader _grader;
        private readonly INavigationService _navigationService;
        private string _actualOutput;
        private double _percentPass;
        private string _errorMessage;
        private CodeProject _codeProject;

        public ProjectViewModel(IVisualStudioService visualStudioService, IConsoleAppGrader grader, INavigationService navigationService)
        {
            _visualStudioService = visualStudioService;
            _grader = grader;
            _navigationService = navigationService;
            CodeProject = new CodeProject();
            CodeProject.CsvCases = "";
            CodeProject.CsvExpectedOutput = "Hello World!";
            TestCommand = new DelegateCommandAsync(Test);
            SubmitCommand = new DelegateCommand(Submit);
        }

        public bool IsReadOnly { get; set; }
        public override async Task InitializeAsync(INavigationParameter parameter)
        {
            //if (parameter["Class"] is Class cls)
            //{
            //    CodeProject = new CodeProject();
            //    CodeProject.ClassId = cls.Id;
                
            //}

            //if (parameter["Project"] is CodeProject project)
            //{
            //    CodeProject = project;
            //}
        }

        public CodeProject CodeProject
        {
            get => _codeProject;
            set => SetProperty(ref _codeProject, value);
        }

        private async void Submit()
        {
            await _navigationService.ToPage("ProjectPublishedView");
        }

        public async Task Test()
        {
            try
            {
                var gradeCases = new List<IGradeCase>();
                var codes = await _visualStudioService.GetCSharpFilesAsync();
                var inputs = CodeProject.CsvCases.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var outputs = CodeProject.CsvExpectedOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                for (var index = 0; index < outputs.Length; index++)
                {
                    var input = "";
                    if (index < inputs.Length)
                    {
                        input = inputs[index];
                    }

                    var output = outputs[index];


                    string[] inputArray = new string[0];
                    if (!string.IsNullOrEmpty(input))
                    {
                        inputArray = input.Split(new[] { "," }, StringSplitOptions.None);
                    }

                    string[] outputArray = new string[0];

                    if (!string.IsNullOrEmpty(output))
                    {
                        outputArray = output.Split(new[] { "," }, StringSplitOptions.None);
                    }

                    gradeCases.Add(new GradeCase(inputArray, outputArray));
                }


                var result = await _grader.Grade(codes, gradeCases);

                PercentPass = result.PercentPassing;
                ErrorMessage = "";
                ActualOutput = "";
                foreach (var resultCaseResult in result.CaseResults)
                {
                    ErrorMessage += resultCaseResult.ErrorMessage + "\r\n";
                    if (resultCaseResult.ActualOutput.Any())
                        this.ActualOutput += resultCaseResult.ActualOutput.Aggregate((f, s) => f + "," + s) + "\r\n";

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ErrorMessage = e.Message;
            }

        }

        public double PercentPass
        {
            get => _percentPass;
            set => SetProperty(ref _percentPass, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

       

        public string ActualOutput
        {
            get => _actualOutput;
            private set => SetProperty(ref _actualOutput, value);
        }

        public DelegateCommandAsync TestCommand { get; set; }
        public DelegateCommand SubmitCommand { get; set; }
    }
}