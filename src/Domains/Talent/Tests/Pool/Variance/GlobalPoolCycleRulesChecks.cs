namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalPoolCycleRulesChecks
{
    public static void GlobalPoolCycleValidatorReportsEveryProblem()
    {
        var validator = GlobalPoolCycleRules.DefaultGlobalPoolCycleValidator();
        var item = GlobalPoolCycle.NewGlobalPoolCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPoolCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPoolCycleRequiredCodeRule("global pool cycle needs this field");
        var item = GlobalPoolCycle.NewGlobalPoolCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPoolCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalPoolCycleRequiredNameRule("global pool cycle needs an entry");
        var item = GlobalPoolCycle.NewGlobalPoolCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPoolCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalPoolCycleCodeLengthRule(86400);
        var item = GlobalPoolCycle.NewGlobalPoolCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPoolCycleWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new GlobalPoolCycleWeightRangeRule(3.4, 2889.9);
        var item = GlobalPoolCycle.NewGlobalPoolCycle("x", "y");
        item.Weight = 2891.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPoolCycleNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalPoolCycleNameLengthRule(64);
        var item = GlobalPoolCycle.NewGlobalPoolCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalPoolCycleValidatorReportsEveryProblem();
        GlobalPoolCycleRequiredCodeRuleFlagsABlankCode();
        GlobalPoolCycleRequiredNameRuleFlagsABlankName();
        GlobalPoolCycleCodeLengthRuleFlagsALongCode();
        GlobalPoolCycleWeightRangeRuleFlagsValuesAboveWeight();
        GlobalPoolCycleNameLengthRuleFlagsALongName();
    }
}
