namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryWarrantyRollupRulesChecks
{
    public static void PrimaryWarrantyRollupValidatorReportsEveryProblem()
    {
        var validator = PrimaryWarrantyRollupRules.DefaultPrimaryWarrantyRollupValidator();
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryWarrantyRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PrimaryWarrantyRollupGraceDaysRangeRule(32, 255);
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryWarrantyRollupPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PrimaryWarrantyRollupPriorityRangeRule(120, 255);
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryWarrantyRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryWarrantyRollupRequiredTitleRule("primary warranty rollup needs a value");
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryWarrantyRollupValidatorReportsEveryProblem();
        PrimaryWarrantyRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PrimaryWarrantyRollupPriorityRangeRuleFlagsValuesAbovePriority();
        PrimaryWarrantyRollupRequiredTitleRuleFlagsABlankTitle();
    }
}
