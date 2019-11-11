#tool "xunit.runner.console"
#addin nuget:?package=Cake.Coverlet

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var debugConfiguration = Argument("configuration", "Debug");
var buildDir = Directory("./build");
var distDir = Directory("./dist");
var solution = "./SplunkTracing.sln";
var library = "./src/SplunkTracing/SplunkTracing.csproj";
var splunkAssemblyInfoFile = "./src/SplunkTracing/Properties/AssemblyInfo.cs";		
var version = EnvironmentVariable("CIRCLE_TAG") ?? "v0.1.0";
version = version.TrimStart('v');
var buildNo = String.IsNullOrWhiteSpace(EnvironmentVariable("CIRCLE_BUILD_NUM")) ? "0" : EnvironmentVariable("CIRCLE_BUILD_NUM");
var semVersion = string.Concat(version + "-" + buildNo);
var transformedVersion = string.Concat(version + "." + buildNo);
if (version.Contains("-"))
{
	transformedVersion = string.Concat(version.Substring(0, version.LastIndexOf("-")) + "." + buildNo);
}
var nuGetApiKey = EnvironmentVariable("NuGet");
var testAssemblyFriendlyName = "SplunkTracing.Tests,PublicKey=002400000480000094000000060200000024000052534131000400000100010099deeadb052e9763d2dc7827700d80e349e5d16585c92416171e6689a4bd38a3acea971d5899d5e2cd4239c3dc799558138e961f8d0f5095fef969672172833868f2cc2d908970370af834beef9dad328182fee2aaf0d0bb568ffd1f829362b88718734541d334c6a2cdf0049f5a0ee5e4962d0db3f49f86bf742f9531bd9c8c";

Task("Clean")
    .Does( ()=> 
{
    CleanDirectory(buildDir);
	CleanDirectory(distDir);
    CleanDirectories("./**/obj/*.*");
    CleanDirectories($"./**/bin/{configuration}/*.*");
	CleanDirectories($"./**/bin/{debugConfiguration}/*.*");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does( ()=> 
{
    DotNetCoreRestore(solution);
});

Task("Build")
	.IsDependentOn("Restore")
    .Does(() =>
	{
		CreateAssemblyInfo(splunkAssemblyInfoFile, new AssemblyInfoSettings {
			Product = "SplunkTracing",
			Version = transformedVersion,
			FileVersion = transformedVersion,
			InformationalVersion = version,
			Copyright = string.Format("Copyright (c) Splunk 2019 - {0}", DateTime.Now.Year),
			InternalsVisibleTo = new List<string>() { testAssemblyFriendlyName }
		});
		var assemblyInfo = ParseAssemblyInfo(splunkAssemblyInfoFile);
		Information("Version: {0}", assemblyInfo.AssemblyVersion);
		Information("File version: {0}", assemblyInfo.AssemblyFileVersion);
		Information("Informational version: {0}", assemblyInfo.AssemblyInformationalVersion);
		MSBuild(library, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.WithProperty("Version", assemblyInfo.AssemblyInformationalVersion)
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		var projects = GetFiles("./test/**/*.csproj");
		var coverletSettings = new CoverletSettings {
			CollectCoverage = true,
			CoverletOutputFormat = CoverletOutputFormat.opencover,
			CoverletOutputDirectory = Directory("./build"),
			CoverletOutputName = $"coverage.xml",
			ExcludeByFile = { "../../src/SplunkTracing/Collector/Collector.cs", "../../src/SplunkTracing/SplunkTracing.cs" }
		};
        foreach(var project in projects)
        {
			DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings {
				Logger = "xunit;LogFilePath=../../build/test_results.xml"
			}, coverletSettings);
        }
});

Task("Publish")
    .IsDependentOn("Test")
	.WithCriteria(() => EnvironmentVariable("CI") == "true")
    .Does(() =>
    {
		var nupkg = GetFiles("./src/SplunkTracing/bin/Release/*.nupkg").First();
		DotNetCoreNuGetPush(nupkg.FullPath, new DotNetCoreNuGetPushSettings {
			Source = "https://www.nuget.org/api/v2/package",
			ApiKey = nuGetApiKey
		});
    });

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);