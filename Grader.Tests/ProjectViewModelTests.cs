﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncToolWindowSample.Models;
using AsyncToolWindowSample.ToolWindows;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;





namespace Grader.Tests
{
    [TestFixture]
    public class ProjectViewModelTests
    {
        private Fixture fixture;
        private Mock<IVisualStudioService> vsMock;
        private ProjectViewModel model;
       [SetUp]
        public void Setup()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization() { ConfigureMembers = true, GenerateDelegates = true });
            fixture.Inject<IConsoleAppGrader>(new ConsoleAppGrader(new CSharpGenerator()));

            vsMock = fixture.Freeze<Mock<IVisualStudioService>>();
             model = fixture.Build<ProjectViewModel>().OmitAutoProperties().Create();

        }
        const string  helloWorldSrc = @"
using System.Collections;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(""Hello World!"");
        }
    }
}";

        const string taxSystemSrc = @"
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Enter Price"");
            var price = double.Parse(Console.ReadLine());

            Console.WriteLine((price * 1.10));
        }
    }
}
";
        [Test]
        public async Task HelloWorld_Should_Pass()
        {
            
            var output = "Hello World!";

            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent(){Content = helloWorldSrc} }.AsEnumerable()));

            model.CodeProject.CsvExpectedOutput = output;

            await model.TestCommand.ExecuteAsync();


            model.Submission.Grade.Should().Be(1);
        }


        [Test]
        public async Task InvalidNumberOfInputs_ShouldGiveErrorMessage()
        {

            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent(){Content = taxSystemSrc}}.AsEnumerable()));

            await model.TestCommand.ExecuteAsync();
            model.ErrorMessage.Should().Be("Case 1: Missing input\r\n");
        }


        [Test]
        public async Task NoInputOutput_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] {new FileContent() {Content = helloWorldSrc}}.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = null;
            model.CodeProject.CsvCases = null;
            await model.TestCommand.ExecuteAsync();

            model.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task InputWithNoOutput_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystemSrc } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = null;
            model.CodeProject.CsvCases = "1,2\r\n1,2";
            await model.TestCommand.ExecuteAsync();

            model.Submission.Grade.Should().Be(1);
            model.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public void InputWithoutOutput_Count_Should_BeTwo()
        {
            var result = ProjectViewModel.ConvertToGradeCases(new string[0], new[] {"1,2", "1,2"});
            result.Should().HaveCount(2);
        }

        [Test]
        public void NegationSyntax_Should_NotChangeContent()
        {
            var result = ProjectViewModel.ConvertToGradeCases( new[] { "!1,!2" }, new[] { "1,2" });

            result.Should().HaveCount(1);

            result[0].ExpectedOutputs.Should().HaveCount(2);
            result[0].ExpectedOutputs.All(p=>p.Negate).Should().BeTrue();
            result[0].ExpectedOutputs.All(p => int.TryParse(p, out var t)).Should().BeTrue();
        }

        [Test]
        public void HintSyntax_Should_NotChangeContent()
        {
            var result = ProjectViewModel.ConvertToGradeCases( new[] { "1[did you handle one?],2[did you handle two?]" }, new[] { "1,2" });

            result.Should().HaveCount(1);

            result[0].ExpectedOutputs.Should().HaveCount(2);
            result[0].ExpectedOutputs.Select(p => p.Hint.Should().NotBeNullOrEmpty()).ToList();
            result[0].ExpectedOutputs.Select(p => int.TryParse(p, out var t).Should().BeTrue()).ToList();
        }

        [Test]
        public async Task Fail_Should_ShowHint()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
               .Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystem } }.AsEnumerable()));
            model.CodeProject.CsvCases = @"10,10
20,10
100,15";
            model.CodeProject.CsvExpectedOutput = @"$10,10%,$1,$11
$20,10%,$2,$22
$100,15%,$15,$test[Did you see this?]";

            await model.TestCommand.ExecuteAsync();
            System.Console.WriteLine(model.ErrorMessage);
            model.ErrorMessage.Should().NotBeNullOrWhiteSpace();

            model.Submission.Grade.Should().BeInRange(.6, .7);
            model.ErrorMessage.Should().Contain("Did you see this?");
            model.ErrorMessage.Should().NotContain("[");
            model.ErrorMessage.Should().NotContain("]");
        }



        private const string longestWordProgram = @"
using System;
using System.Linq;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var item = Console.ReadLine();

            var words = item.Split(' ');

            var longestWord = words
                .FirstOrDefault(p => p.ToList().Count == words.Max(r => r.Length));

            Console.WriteLine(longestWord);
        }
    }
}";

        private const string longestWordProgramBestSolution = @"using System;
using System.Linq;

class MainClass {
  public static string LongestWord(string sen) { 
  
    string[] words = sen.Split(' ');


    return words.OrderByDescending( s => s.Length ).First();;
            
  }

  static void Main() {  
    // keep this function call here
    Console.WriteLine(LongestWord(Console.ReadLine()));
  } 
   
}";

        private const string emptyMain = @"class MainClass {static void Main(){}}";

        [Test]
        public async Task MainWithNoParameters_Should_NotThrowException()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = emptyMain } }.AsEnumerable()));
            model.CodeProject.CsvCases = "";
            model.CodeProject.CsvExpectedOutput = "";

            await model.TestCommand.ExecuteAsync();
            model.Submission.Grade.Should().Be(1);
            model.ErrorMessage.Should().BeNullOrEmpty();
        }



        [TestCase(longestWordProgram, .75)]
        [TestCase(longestWordProgramBestSolution, .75)]
        public async Task LongestWordProgram_Should_Pass(string src, double result)
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = src } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = @"method
something
love
time
";
            model.CodeProject.CsvCases = @"test method
super long something
I love dogs
fun& time
";

            await model.TestCommand.ExecuteAsync();
            model.ErrorMessage.Should().Contain("Case 4: Failed\r\n");

            model.Submission.Grade.Should().Be(result);
        }
        string taxSystem = @"
using System;

class MainClass
{
    

    static void Main()
    {
            Console.WriteLine(""Enter price"");
            var price = double.Parse(Console.ReadLine());
            Console.WriteLine(""Enter tax percent"");
            var tax = double.Parse(Console.ReadLine());

            Console.WriteLine($""Price: ${price}"");
            Console.WriteLine($""Tax Percent: {tax}%"");
            Console.WriteLine($""Tax Cost: ${tax / 100 * price}"");
            Console.WriteLine($""Total Cost: ${price + (tax / 100 * price)}"");

        }

    }
";
        [Test]
        public async Task TaxSystem_FailingCase_ShouldNotifyWhichCaseFailed()
        {
            
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystem } }.AsEnumerable()));
            model.CodeProject.CsvCases = @"10,10
20,10
100,15";
            model.CodeProject.CsvExpectedOutput = @"$10,10%,$1,$11
$20,10%,$2,$22
$100,15%,$15,$test";

            await model.TestCommand.ExecuteAsync();
            System.Console.WriteLine(model.ErrorMessage);
            model.ErrorMessage.Should().NotBeNullOrWhiteSpace();

            model.Submission.Grade.Should().BeInRange(.6,.7);
        }





        [Test]
        public void LongestWord_Should_Pass()
        {
            var results = ProjectViewModel.ConvertToGradeCases(new[] {"method", "something", "love", "time"},
                new[] {"test method", "super long something", "I love dogs", "fun& time"});


            results.Should().HaveCount(4);
        }



        [Test]
        public async Task InvalidNumberOfInputs_Should_NotFailAllCases()
        {
            

            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent(){Content = taxSystemSrc}}.AsEnumerable()));

            model.CodeProject.CsvCases = "10\r\n12\r\ntest";
            model.CodeProject.CsvExpectedOutput = "11\r\n13.2\r\n";

            await model.Test();

            model.Submission.Grade.Should().BeInRange(.65, .67);
        }
    }
}