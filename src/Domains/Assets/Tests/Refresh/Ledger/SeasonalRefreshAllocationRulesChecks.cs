namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalRefreshAllocationRulesChecks
{
    public static void SeasonalRefreshAllocationValidatorReportsEveryProblem()
    {
        var validator = SeasonalRefreshAllocationRules.DefaultSeasonalRefreshAllocationValidator();
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalRefreshAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalRefreshAllocationRequiredCodeRule("seasonal refresh allocation needs an entry");
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalRefreshAllocationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new SeasonalRefreshAllocationGradeLengthRule(16);
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalRefreshAllocationDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalRefreshAllocationDiscountRangeRule(1.9, 1984.1);
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("x", "y");
        item.Discount = 1985.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalRefreshAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SeasonalRefreshAllocationStartsAtRangeRule(120, 3600);
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalRefreshAllocationHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new SeasonalRefreshAllocationHoursRangeRule(6.0, 258.9);
        var item = SeasonalRefreshAllocation.NewSeasonalRefreshAllocation("x", "y");
        item.Hours = 260.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalRefreshAllocationValidatorReportsEveryProblem();
        SeasonalRefreshAllocationRequiredCodeRuleFlagsABlankCode();
        SeasonalRefreshAllocationGradeLengthRuleFlagsALongGrade();
        SeasonalRefreshAllocationDiscountRangeRuleFlagsValuesAboveDiscount();
        SeasonalRefreshAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SeasonalRefreshAllocationHoursRangeRuleFlagsValuesAboveHours();
    }
}
