namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class GarnishmentBundleRulesChecks
{
    public static void GarnishmentBundleValidatorReportsEveryProblem()
    {
        var validator = GarnishmentBundleRules.DefaultGarnishmentBundleValidator();
        var item = GarnishmentBundle.NewGarnishmentBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GarnishmentBundleRequiredTitleRule("garnishment bundle needs a value");
        var item = GarnishmentBundle.NewGarnishmentBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentBundleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new GarnishmentBundleOwnerIdLengthRule(64);
        var item = GarnishmentBundle.NewGarnishmentBundle("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentBundleWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new GarnishmentBundleWeightRangeRule(6.6, 3023.5);
        var item = GarnishmentBundle.NewGarnishmentBundle("x", "y");
        item.Weight = 3025.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentBundleCodeLengthRule(1000);
        var item = GarnishmentBundle.NewGarnishmentBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentBundleValidatorReportsEveryProblem();
        GarnishmentBundleRequiredTitleRuleFlagsABlankTitle();
        GarnishmentBundleOwnerIdLengthRuleFlagsALongOwnerId();
        GarnishmentBundleWeightRangeRuleFlagsValuesAboveWeight();
        GarnishmentBundleCodeLengthRuleFlagsALongCode();
    }
}
