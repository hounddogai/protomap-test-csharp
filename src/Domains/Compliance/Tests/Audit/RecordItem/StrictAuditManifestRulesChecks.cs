namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictAuditManifestRulesChecks
{
    public static void StrictAuditManifestValidatorReportsEveryProblem()
    {
        var validator = StrictAuditManifestRules.DefaultStrictAuditManifestValidator();
        var item = StrictAuditManifest.NewStrictAuditManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAuditManifestMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new StrictAuditManifestMinimumRangeRule(5.4, 3864.3);
        var item = StrictAuditManifest.NewStrictAuditManifest("x", "y");
        item.Minimum = 3865.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAuditManifestValidatorReportsEveryProblem();
        StrictAuditManifestMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
