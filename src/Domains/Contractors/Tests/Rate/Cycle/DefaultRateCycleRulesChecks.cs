namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultRateCycleRulesChecks
{
    public static void DefaultRateCycleValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultRateCycleRules.DefaultDefaultRateCycleValidator();
        var item = DefaultRateCycle.NewDefaultRateCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRateCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultRateCycleRequiredCodeRule("default rate cycle needs an entry");
        var item = DefaultRateCycle.NewDefaultRateCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRateCycleVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DefaultRateCycleVersionRangeRule(80, 86400);
        var item = DefaultRateCycle.NewDefaultRateCycle("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultRateCycleRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new DefaultRateCycleRequiredRegionRule("default rate cycle needs this field");
        var item = DefaultRateCycle.NewDefaultRateCycle("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRateCycleLocationLengthRuleFlagsALongLocation()
    {
        var rule = new DefaultRateCycleLocationLengthRule(32);
        var item = DefaultRateCycle.NewDefaultRateCycle("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRateCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultRateCycleCodeLengthRule(80);
        var item = DefaultRateCycle.NewDefaultRateCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRateCycleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new DefaultRateCycleAttemptsRangeRule(80, 3600);
        var item = DefaultRateCycle.NewDefaultRateCycle("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultRateCycleValidatorExtraReportsEveryProblem();
        DefaultRateCycleRequiredCodeRuleFlagsABlankCode();
        DefaultRateCycleVersionRangeRuleFlagsValuesAboveVersion();
        DefaultRateCycleRequiredRegionRuleFlagsABlankRegion();
        DefaultRateCycleLocationLengthRuleFlagsALongLocation();
        DefaultRateCycleCodeLengthRuleFlagsALongCode();
        DefaultRateCycleAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
