# Visual Studio GradeBook
Simplyifing grading code in visual studio.  No more sending compressed files, downlowding, and opening student's code.

## Features

- Teachers can create master programs to compare to students
- Teachers can create inputs for their program that will be tested against the student's program.  
- Teachers can then create assignments for students to complete
- When students create the program, they'll be able to test it within visual studio with the inputs the teacher created.  This inputs can be made invisible or visible.
- Students will get a percent of passing cases based on the inputs the teacher provided
- Once the students are finished with their program, 
  they can submit the program within visual studio
- You can use [VsGradeBook.com]("www.VsGradeBook.com") to see the APIs
  and options to submit the code

## Integrations

- Submit with GitHub
- Test with Unit Tests
- OAuth for authentication
- Integrate assignments with Canvas


# Async Tool Window example (Original Documentation)

**Applies to Visual Studio 2017.6 and newer**

This sample shows how to provide an Async Tool Window in a Visual Studio extension.

Clone the repo to test out the sample in Visual Studio 2017 yourself.

![Tool Window](art/tool-window.png)

## Specify minimum supported version
Since Async Tool Window support is new in Visual Studio 2017 Update 6, we need to specify that our extension requires that version or newer. We do that in the .vsixmanifest file like so:

```xml
<InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[15.0.27413, 16.0)" />
```

*15.0.27413* is the full version string of Visual Studio 2017 Update 6.

See the full sample [.vsixmanifest file](src/source.extension.vsixmanifest).

## This sample
The code in this sample contains the concepts:

1. [Custom Tool Window Pane](src/ToolWindows/SampleToolWindow.cs)
2. [XAML control](src/ToolWindows/SampleToolWindowControl.xaml) for the pane
3. [Custom command](src/Commands/ShowToolWindow.cs) that can show the tool window
4. [AsyncPackage class](src/MyPackage.cs) that glues it all together

Follow the links above directly into the source code to see how it is all hooked up.

## Further reading
Read the docs for all the details surrounding these scenarios.

* [VSCT Schema Reference](https://docs.microsoft.com/en-us/visualstudio/extensibility/vsct-xml-schema-reference)
* [Use AsyncPackage with background load](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background)
* [Custom command sample](https://github.com/madskristensen/CustomCommandSample)