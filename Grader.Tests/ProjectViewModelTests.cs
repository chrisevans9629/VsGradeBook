﻿using System;
using System.Collections.Generic;
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
        private ConsoleAppGrader consoleAppGrader;
        [SetUp]
        public void Setup()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization() { ConfigureMembers = true, GenerateDelegates = true });
            consoleAppGrader = new ConsoleAppGrader(new CSharpGenerator());
            fixture.Inject<IConsoleAppGrader>(consoleAppGrader);

            vsMock = fixture.Freeze<Mock<IVisualStudioService>>();
            vsMock.Setup(p => p.GetReferences()).Returns(Task.FromResult<IEnumerable<string>>(null));
            model = fixture.Build<ProjectViewModel>().OmitAutoProperties().Create();

        }
        const string helloWorldSrc = @"
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
        const string throwExceptionSrc = @"
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            throw new Exception(""Test"");
        }
    }
}
";

        [TestCase("^exTest", true, "")]
        [TestCase("^exNotTest", false, "NotTest")]
        [TestCase("Test", false, "Case 1: Test")]
        public async Task Exception_Should_Do( string output, bool pass, string message)
        {
            var cases = CsvGradeCaseGenerator.ConvertTextToGradeCases("", output);

            var results = await consoleAppGrader.Grade(throwExceptionSrc, cases);

            var caseResult = results.CaseResults.Single();
            caseResult.Pass.Should().Be(pass);
            caseResult.ErrorMessage.Should().Be(message);
        }

        [Test]
        public async Task HelloWorld_Should_Pass()
        {

            var output = "Hello World!";

            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent() { Content = helloWorldSrc } }.AsEnumerable()));

            model.CodeProject.CsvExpectedOutput = output;

            await model.TestCommand.ExecuteAsync();


            model.Submission.Grade.Should().Be(1);
        }


        [Test]
        public async Task InvalidNumberOfInputs_ShouldGiveErrorMessage()
        {

            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystemSrc } }.AsEnumerable()));

            await model.TestCommand.ExecuteAsync();
            model.ErrorMessage.Should().Be("Case 1: Missing input\r\n");
        }


        [Test]
        public async Task NoInputOutput_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = helloWorldSrc } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = null;
            model.CodeProject.CsvCases = null;
            await model.TestCommand.ExecuteAsync();

            model.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public void Input_Output_Should_Match_Cases()
        {
            model.CsvCases = "test";
            model.CsvExpectedOutput = "test";

            var case1 = model.Cases.Should().HaveCount(1).And.Subject.First();
            case1.Inputs.First().Should().Be("test");
            case1.ExpectedOutputs.First().ValueToMatch.Should().Be("test");
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
            var result = CsvGradeCaseGenerator.ConvertToGradeCases(new string[0], new[] { "1,2", "1,2" });
            result.Should().HaveCount(2);
        }





        [Test]
        public void NegationSyntax_Should_NotChangeContent()
        {
            var result = CsvGradeCaseGenerator.ConvertToGradeCases(new[] { "!1,!2" }, new[] { "1,2" });

            result.Should().HaveCount(1);

            result[0].ExpectedOutputs.Should().HaveCount(2);
            result[0].ExpectedOutputs.All(p => p.Negate).Should().BeTrue();
            result[0].ExpectedOutputs.All(p => int.TryParse(p, out var t)).Should().BeTrue();
        }

        [TestCase("","^ex1",true, "1")]
        [TestCase("","1",false, "1")]
        [TestCase("test","^exthis is a test",true, "this is a test")]
        [TestCase("test","^ex\"this is a test\"",true, "this is a test")]
        [TestCase("test", "this is^ex a test", false, "this is^ex a test")]
        public void ExceptionSyntax_Should_SetException(string input, string outputstr, bool isException, string msg)
        {
            var result = CsvGradeCaseGenerator.ConvertTextToGradeCases(input, outputstr);

            var output = result.Single().ExpectedOutputs.Single();
            output.Exception.Should().Be(isException);
            output.ValueToMatch.Should().Be(msg);
        }




        [Test]
        public void HintSyntax_Should_NotChangeContent()
        {
            var result = CsvGradeCaseGenerator.ConvertToGradeCases(new[] { "1[did you handle one?],2[did you handle two?]" }, new[] { "1,2" });

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

        [Test]
        public async Task Negate_Should_Fail()
        {

            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = helloWorldSrc } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = @"!Hello World";

            await model.TestCommand.ExecuteAsync();
            System.Console.WriteLine(model.ErrorMessage);
            model.Submission.Grade.Should().Be(0);
            model.ErrorMessage.Should().Contain("Not Expected 'Hello World'");
        }

        [Test]
        public async Task HelloWorld_CaseInsensitive_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = helloWorldSrc } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = @"^ihello world!";

            await model.TestCommand.ExecuteAsync();
            model.Submission.Grade.Should().Be(1);

        }

        [Test]
        public async Task HelloWorld_Regex_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = helloWorldSrc } }.AsEnumerable()));
            model.CodeProject.CsvExpectedOutput = @"^r[a-zA-Z]";

            await model.TestCommand.ExecuteAsync();
            model.Submission.Grade.Should().Be(1);

        }
        [Test]
        public async Task Negate_Should_Pass()
        {
            vsMock.Setup(p => p.GetCSharpFilesAsync())
                .Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystem } }.AsEnumerable()));
            model.CodeProject.CsvCases = @"10,10
20,10
100,15";
            model.CodeProject.CsvExpectedOutput = @"$10,10%,$1,$11
$20,10%,$2,$22
$100,15%,$15,!$test[Did you see this?]";

            await model.TestCommand.ExecuteAsync();

            model.Submission.Grade.Should().Be(1);
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

            model.Submission.Grade.Should().BeInRange(.6, .7);
        }


        [Test]
        public void VisualCases_Should_MatchTextCases()
        {
            model.CsvCases = @"10,10
20,10
100,15";
            model.CsvExpectedOutput = @"$10,10%,$1,$11
$20,10%,$2,$22
$100,15%,$15,$test";

            model.Cases.Should().HaveCount(3);

        }


        [Test]
        public async Task CreateSubmission_OutputInput_Should_Match()
        {
            var cases = "cases";
            var outputs = "outputs";
            model.Initialize(new NavigationParameter() { { "Project", new CodeProject() { CsvCases = cases, CsvExpectedOutput = outputs } } });

            model.CsvCases.Should().Be(cases);
            model.CsvExpectedOutput.Should().Be(outputs);

        }

        [Test]
        public void QuotationTest()
        {
            var CsvCases = @"""test"", ""test2 "",""test,test"", test3 , , "" "" ,test[hint] ";
            var CsvExpectedOutput = "";

            var result = CsvGradeCaseGenerator.ConvertTextToGradeCases(CsvCases, CsvExpectedOutput);

            var inp = result.First().Inputs;

            inp[0].Should().Be("test");
            inp[1].Should().Be("test2 ");
            inp[2].Should().Be("test,test");
            inp[3].Should().Be(" test3 ");
            inp[4].Should().Be(" ");
            inp[5].Should().Be(" ");

        }

        [TestCase(@"""test """" test2 """)]
        [TestCase(@"""test ""asdf"" test2 """)]
        [TestCase(@"""test")]
        public void ExpectedOutput_Should_Fail(string expected)
        {
            var result = Assert.Throws<ParserException>(() => CsvGradeCaseGenerator.ConvertTextToGradeCases("", expected));
            result.Exceptions.Should().HaveCount(1);
        }

        [Test]
        public void TwoErrors_Should_ThrowTwoExceptions()
        {
            var exception = Assert.Throws<ParserException>(() =>
                CsvGradeCaseGenerator.ConvertTextToGradeCases(@"""test """" test2 """, @"""test """" test2 """));

            exception.Exceptions.Should().HaveCount(2);
        }


        [Test]
        public void QuoteWithTrailingSpace_Should_HaveOneCase()
        {
            model.CsvExpectedOutput = @"""asasdf[]""       ";
            model.Cases.Should().HaveCount(1);
            model.Cases.First().ExpectedOutputs.Should().HaveCount(1);
        }

        [Test]
        public void MessageWithQuotations()
        {
            model.CsvExpectedOutput = @"""test""[this is a message],""test2[test]""";

            var result = CsvGradeCaseGenerator.ConvertTextToGradeCases(model.CsvCases, model.CsvExpectedOutput);

            var ot = result.First().ExpectedOutputs;
            ot[0].ValueToMatch.Should().Be("test");
            ot[0].Hint.Should().Be("this is a message");
            ot[1].ValueToMatch.Should().Be("test2[test]");
            ot[1].Hint.Should().BeNullOrEmpty();
        }


        [Test]
        public void NegateWithQuotations()
        {
            model.CsvExpectedOutput = @"!""test"",""!test2""";


            var result = CsvGradeCaseGenerator.ConvertTextToGradeCases(model.CsvCases, model.CsvExpectedOutput);

            var ot = result.First().ExpectedOutputs;
            ot[0].ValueToMatch.Should().Be("test");
            ot[0].Negate.Should().BeTrue();
            ot[1].ValueToMatch.Should().Be("!test2");
            ot[1].Negate.Should().BeFalse();
        }



        [Test]
        public void LongestWord_Should_Pass()
        {
            var results = CsvGradeCaseGenerator.ConvertToGradeCases(new[] { "method", "something", "love", "time" },
                new[] { "test method", "super long something", "I love dogs", "fun& time" });


            results.Should().HaveCount(4);
        }



        [Test]
        public async Task InvalidNumberOfInputs_Should_NotFailAllCases()
        {


            vsMock.Setup(p => p.GetCSharpFilesAsync()).Returns(Task.FromResult(new[] { new FileContent() { Content = taxSystemSrc } }.AsEnumerable()));

            model.CodeProject.CsvCases = "10\r\n12\r\ntest";
            model.CodeProject.CsvExpectedOutput = "11\r\n13.2\r\n";

            await model.Test();

            model.Submission.Grade.Should().BeInRange(.65, .67);
        }
    }
}