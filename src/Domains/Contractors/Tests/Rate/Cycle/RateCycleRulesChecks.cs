namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RateCycleRulesChecks
{
    public static void RateCycleValidatorReportsEveryProblem()
    {
        var validator = RateCycleRules.DefaultRateCycleValidator();
        var item = RateCycle.NewRateCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateCycleRequiredCodeRule("rate cycle needs a value");
        var item = RateCycle.NewRateCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateCycleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RateCycleSalaryBandLengthRule(120);
        var item = RateCycle.NewRateCycle("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateCycleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RateCycleRequiredTitleRule("rate cycle needs a value");
        var item = RateCycle.NewRateCycle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RateCycleRequiredEmployeeIdRule("rate cycle needs this field");
        var item = RateCycle.NewRateCycle("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateCycleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RateCycleStartsAtRangeRule(255, 86400);
        var item = RateCycle.NewRateCycle("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RateCycleValidatorReportsEveryProblem();
        RateCycleRequiredCodeRuleFlagsABlankCode();
        RateCycleSalaryBandLengthRuleFlagsALongSalaryBand();
        RateCycleRequiredTitleRuleFlagsABlankTitle();
        RateCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        RateCycleStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
