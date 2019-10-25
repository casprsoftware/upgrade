//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{   
    // bin
    CleanDirectories("src/*/bin/" + configuration);
    // dist
    CleanDirectories("dist/");    
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    var projects = GetFiles("src/**/*.csproj");
    foreach (var project in projects)
    {
        DotNetCoreBuild(project.FullPath, settings);    
    }
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{    
    var projects = GetFiles("test/**/*.csproj");
    foreach(var project in projects)
    {
        DotNetCoreTest(
            project.FullPath,
            new DotNetCoreTestSettings()
            {
                Configuration = configuration                
            });
    }
    // todo run xunit https://andrewlock.net/running-tests-with-dotnet-xunit-using-cake/    
});

Task("Default")
    .IsDependentOn("Build")
    .Does(()=>{
        
        var settings = new DotNetCorePublishSettings
        {
            Framework = "netcoreapp2.1",
            Configuration = configuration,
            OutputDirectory = "dist/console/",
            NoBuild = true
        };

        DotNetCorePublish("src/Upgrade.Console/Upgrade.Console.csproj", settings);
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);