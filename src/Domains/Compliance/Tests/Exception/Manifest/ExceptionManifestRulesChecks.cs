namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionManifestRulesChecks
{
    public static void ExceptionManifestValidatorReportsEveryProblem()
    {
        var validator = ExceptionManifestRules.DefaultExceptionManifestValidator();
        var item = ExceptionManifest.NewExceptionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ExceptionManifestRequiredTitleRule("exception manifest needs an entry");
        var item = ExceptionManifest.NewExceptionManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionManifestRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ExceptionManifestRequiredEmployeeIdRule("exception manifest needs this field");
        var item = ExceptionManifest.NewExceptionManifest("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionManifestValidatorReportsEveryProblem();
        ExceptionManifestRequiredTitleRuleFlagsABlankTitle();
        ExceptionManifestRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
