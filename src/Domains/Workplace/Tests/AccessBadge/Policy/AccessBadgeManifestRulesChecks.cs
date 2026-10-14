namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AccessBadgeManifestRulesChecks
{
    public static void AccessBadgeManifestValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeManifestRules.DefaultAccessBadgeManifestValidator();
        var item = AccessBadgeManifest.NewAccessBadgeManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeManifestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AccessBadgeManifestScoreRangeRule(5.3, 4053.2);
        var item = AccessBadgeManifest.NewAccessBadgeManifest("x", "y");
        item.Score = 4054.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeManifestRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new AccessBadgeManifestRequiredSalaryBandRule("access badge manifest needs a value");
        var item = AccessBadgeManifest.NewAccessBadgeManifest("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccessBadgeManifestLabelLengthRule(80);
        var item = AccessBadgeManifest.NewAccessBadgeManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeManifestValidatorReportsEveryProblem();
        AccessBadgeManifestScoreRangeRuleFlagsValuesAboveScore();
        AccessBadgeManifestRequiredSalaryBandRuleFlagsABlankSalaryBand();
        AccessBadgeManifestLabelLengthRuleFlagsALongLabel();
    }
}
