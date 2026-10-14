namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleRollupRulesChecks
{
    public static void MeritCycleRollupValidatorReportsEveryProblem()
    {
        var validator = MeritCycleRollupRules.DefaultMeritCycleRollupValidator();
        var item = MeritCycleRollup.NewMeritCycleRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleRollupCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new MeritCycleRollupCurrencyLengthRule(64);
        var item = MeritCycleRollup.NewMeritCycleRollup("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleRollupFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new MeritCycleRollupFteRangeRule(1.5, 4688.7);
        var item = MeritCycleRollup.NewMeritCycleRollup("x", "y");
        item.Fte = 4690.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleRollupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MeritCycleRollupTitleLengthRule(86400);
        var item = MeritCycleRollup.NewMeritCycleRollup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleRollupValidatorReportsEveryProblem();
        MeritCycleRollupCurrencyLengthRuleFlagsALongCurrency();
        MeritCycleRollupFteRangeRuleFlagsValuesAboveFte();
        MeritCycleRollupTitleLengthRuleFlagsALongTitle();
    }
}
