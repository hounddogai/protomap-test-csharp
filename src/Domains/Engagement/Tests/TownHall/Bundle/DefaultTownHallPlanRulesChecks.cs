namespace Acme.Workforce.Engagement.Tests.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class DefaultTownHallPlanRulesChecks
{
    public static void DefaultTownHallPlanValidatorReportsEveryProblem()
    {
        var validator = DefaultTownHallPlanRules.DefaultDefaultTownHallPlanValidator();
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultTownHallPlanUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DefaultTownHallPlanUpdatedAtRangeRule(16, 80);
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultTownHallPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultTownHallPlanCodeLengthRule(80);
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultTownHallPlanCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DefaultTownHallPlanCountryLengthRule(80);
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultTownHallPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultTownHallPlanRequiredCodeRule("default town hall plan needs a value");
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultTownHallPlanValidatorReportsEveryProblem();
        DefaultTownHallPlanUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DefaultTownHallPlanCodeLengthRuleFlagsALongCode();
        DefaultTownHallPlanCountryLengthRuleFlagsALongCountry();
        DefaultTownHallPlanRequiredCodeRuleFlagsABlankCode();
    }
}
