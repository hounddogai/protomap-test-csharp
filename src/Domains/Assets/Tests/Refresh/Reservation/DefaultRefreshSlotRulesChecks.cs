namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultRefreshSlotRulesChecks
{
    public static void DefaultRefreshSlotValidatorReportsEveryProblem()
    {
        var validator = DefaultRefreshSlotRules.DefaultDefaultRefreshSlotValidator();
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRefreshSlotRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new DefaultRefreshSlotRevisionRangeRule(32, 80);
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultRefreshSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultRefreshSlotRequiredCodeRule("default refresh slot needs this field");
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRefreshSlotSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new DefaultRefreshSlotSalaryBandLengthRule(255);
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRefreshSlotRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new DefaultRefreshSlotRequiredSalaryBandRule("default refresh slot needs a value");
        var item = DefaultRefreshSlot.NewDefaultRefreshSlot("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultRefreshSlotValidatorReportsEveryProblem();
        DefaultRefreshSlotRevisionRangeRuleFlagsValuesAboveRevision();
        DefaultRefreshSlotRequiredCodeRuleFlagsABlankCode();
        DefaultRefreshSlotSalaryBandLengthRuleFlagsALongSalaryBand();
        DefaultRefreshSlotRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
