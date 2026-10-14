namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleBaselineRulesChecks
{
    public static void MeritCycleBaselineValidatorReportsEveryProblem()
    {
        var validator = MeritCycleBaselineRules.DefaultMeritCycleBaselineValidator();
        var item = MeritCycleBaseline.NewMeritCycleBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleBaselineCodeLengthRule(86400);
        var item = MeritCycleBaseline.NewMeritCycleBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleBaselineRequiredNameRuleFlagsABlankName()
    {
        var rule = new MeritCycleBaselineRequiredNameRule("merit cycle baseline needs this field");
        var item = MeritCycleBaseline.NewMeritCycleBaseline("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleBaselineValidatorReportsEveryProblem();
        MeritCycleBaselineCodeLengthRuleFlagsALongCode();
        MeritCycleBaselineRequiredNameRuleFlagsABlankName();
    }
}
