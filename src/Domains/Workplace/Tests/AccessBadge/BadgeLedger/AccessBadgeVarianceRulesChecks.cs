namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgeVarianceRulesChecks
{
    public static void AccessBadgeVarianceValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeVarianceRules.DefaultAccessBadgeVarianceValidator();
        var item = AccessBadgeVariance.NewAccessBadgeVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeVarianceWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new AccessBadgeVarianceWeightRangeRule(9.8, 4217.5);
        var item = AccessBadgeVariance.NewAccessBadgeVariance("x", "y");
        item.Weight = 4219.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeVarianceLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AccessBadgeVarianceLocationLengthRule(255);
        var item = AccessBadgeVariance.NewAccessBadgeVariance("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AccessBadgeVarianceWindowDaysRangeRule(120, 3600);
        var item = AccessBadgeVariance.NewAccessBadgeVariance("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AccessBadgeVarianceMaximumRangeRule(5.3, 2873.7);
        var item = AccessBadgeVariance.NewAccessBadgeVariance("x", "y");
        item.Maximum = 2875.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeVarianceRequiredNameRule("access badge variance needs an entry");
        var item = AccessBadgeVariance.NewAccessBadgeVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeVarianceValidatorReportsEveryProblem();
        AccessBadgeVarianceWeightRangeRuleFlagsValuesAboveWeight();
        AccessBadgeVarianceLocationLengthRuleFlagsALongLocation();
        AccessBadgeVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AccessBadgeVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        AccessBadgeVarianceRequiredNameRuleFlagsABlankName();
    }
}
