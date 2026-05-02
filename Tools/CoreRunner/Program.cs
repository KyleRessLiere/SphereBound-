using Spherebound.CoreCombatLoop.Verification;
using System.IO;

if (args.Length > 0)
{
    Console.Error.WriteLine("Usage: dotnet run --project Tools/CoreRunner/CoreRunner.csproj");
    return 2;
}

var overallSuccess = true;
var projectRoot = Directory.GetCurrentDirectory();
overallSuccess &= RunCombatLoopSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateCombatLoopSuiteReport());
overallSuccess &= RunCombatBehaviorSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateCombatBehaviorSuiteReport());
overallSuccess &= RunAbilityDefinitionSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateAbilityDefinitionSuiteReport());
overallSuccess &= RunScenarioRunnerSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateScenarioRunnerSuiteReport());
overallSuccess &= RunUnityDebugActionSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateUnityDebugActionSuiteReport());
overallSuccess &= RunCombatRuntimeUiDataSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateCombatRuntimeUiDataSuiteReport());
overallSuccess &= RunCombatDebugSurfaceSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateCombatDebugSurfaceSuiteReport());
overallSuccess &= RunCombatDebugFileOutputSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateCombatDebugFileOutputSuiteReport());
overallSuccess &= RunVerifierLogOutputSuite();
WriteVerifierLogs(projectRoot, VerifierFileReportFactory.CreateVerifierLogOutputSuiteReport());

if (overallSuccess)
{
    Console.WriteLine("[overall-pass] All verifier suites passed.");
    return 0;
}

Console.Error.WriteLine("[overall-fail] One or more verifier suites failed.");
return 1;

static bool RunCombatLoopSuite()
{
    const string suiteName = "CombatLoopVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = CombatLoopVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunScenarioRunnerSuite()
{
    var report = ScenarioRunnerVerifier.CreateReport();

    Console.WriteLine($"[suite] {report.SuiteName}");

    foreach (var scenarioCheck in report.ScenarioChecks)
    {
        var scenarioRun = scenarioCheck.ScenarioRun;
        Console.WriteLine($"  [scenario] {scenarioRun.Scenario.Name}");
        foreach (var logLine in scenarioRun.LogLines)
        {
            Console.WriteLine($"    {logLine}");
        }

        if (scenarioRun.Verification.Succeeded)
        {
            Console.WriteLine("  [scenario-pass] verification");
        }
        else
        {
            Console.WriteLine("  [scenario-fail] verification");
            foreach (var failure in scenarioRun.Verification.Failures)
            {
                Console.WriteLine($"    {failure.Code}: {failure.Message}");
            }
        }
    }

    foreach (var check in report.CompletedChecks)
    {
        Console.WriteLine($"  [pass] {check}");
    }

    if (report.Succeeded)
    {
        Console.WriteLine($"[suite-pass] {report.SuiteName}");
        return true;
    }

    Console.Error.WriteLine($"[suite-fail] {report.SuiteName}");
    foreach (var failure in report.CheckFailures)
    {
        Console.Error.WriteLine($"  {failure}");
    }

    return false;
}

static bool RunCombatBehaviorSuite()
{
    const string suiteName = "CombatBehaviorVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = CombatBehaviorVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunAbilityDefinitionSuite()
{
    const string suiteName = "AbilityDefinitionVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = AbilityDefinitionVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunUnityDebugActionSuite()
{
    const string suiteName = "UnityDebugActionVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = UnityDebugActionVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunCombatRuntimeUiDataSuite()
{
    const string suiteName = "CombatRuntimeUiDataVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = CombatRuntimeUiDataVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunCombatDebugSurfaceSuite()
{
    const string suiteName = "CombatDebugSurfaceVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = CombatDebugSurfaceVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunCombatDebugFileOutputSuite()
{
    const string suiteName = "CombatDebugFileOutputVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = CombatDebugFileOutputVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static bool RunVerifierLogOutputSuite()
{
    const string suiteName = "VerifierLogOutputVerifier";
    Console.WriteLine($"[suite] {suiteName}");

    try
    {
        var checks = VerifierLogOutputVerifier.RunAll();
        foreach (var check in checks)
        {
            Console.WriteLine($"  [pass] {check}");
        }

        Console.WriteLine($"[suite-pass] {suiteName}");
        return true;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[suite-fail] {suiteName}");
        Console.Error.WriteLine($"  {exception.GetType().Name}: {exception.Message}");
        return false;
    }
}

static void WriteVerifierLogs(string projectRoot, VerifierSuiteFileReport suiteReport)
{
    VerifierLogWriter.WriteSuite(projectRoot, suiteReport);
}
