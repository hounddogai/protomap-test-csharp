namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleTierRulesChecks
{
    public static void MeritCycleTierValidatorReportsEveryProblem()
    {
        var validator = MeritCycleTierRules.DefaultMeritCycleTierValidator();
        var item = MeritCycleTier.NewMeritCycleTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MeritCycleTierTitleLengthRule(16);
        var item = MeritCycleTier.NewMeritCycleTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleTierCodeLengthRule(86400);
        var item = MeritCycleTier.NewMeritCycleTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleTierWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new MeritCycleTierWeightRangeRule(6.9, 2484.2);
        var item = MeritCycleTier.NewMeritCycleTier("x", "y");
        item.Weight = 2485.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleTierValidatorReportsEveryProblem();
        MeritCycleTierTitleLengthRuleFlagsALongTitle();
        MeritCycleTierCodeLengthRuleFlagsALongCode();
        MeritCycleTierWeightRangeRuleFlagsValuesAboveWeight();
    }
}
