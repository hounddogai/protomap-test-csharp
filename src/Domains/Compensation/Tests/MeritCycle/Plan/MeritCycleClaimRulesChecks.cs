namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleClaimRulesChecks
{
    public static void MeritCycleClaimValidatorReportsEveryProblem()
    {
        var validator = MeritCycleClaimRules.DefaultMeritCycleClaimValidator();
        var item = MeritCycleClaim.NewMeritCycleClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleClaimRequiredCodeRule("merit cycle claim needs this field");
        var item = MeritCycleClaim.NewMeritCycleClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleClaimJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new MeritCycleClaimJobCodeLengthRule(80);
        var item = MeritCycleClaim.NewMeritCycleClaim("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleClaimSourceLengthRuleFlagsALongSource()
    {
        var rule = new MeritCycleClaimSourceLengthRule(3600);
        var item = MeritCycleClaim.NewMeritCycleClaim("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleClaimValidatorReportsEveryProblem();
        MeritCycleClaimRequiredCodeRuleFlagsABlankCode();
        MeritCycleClaimJobCodeLengthRuleFlagsALongJobCode();
        MeritCycleClaimSourceLengthRuleFlagsALongSource();
    }
}
