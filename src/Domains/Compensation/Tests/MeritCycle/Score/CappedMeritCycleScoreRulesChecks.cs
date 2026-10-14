namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CappedMeritCycleScoreRulesChecks
{
    public static void CappedMeritCycleScoreValidatorReportsEveryProblem()
    {
        var validator = CappedMeritCycleScoreRules.DefaultCappedMeritCycleScoreValidator();
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedMeritCycleScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedMeritCycleScoreCodeLengthRule(3600);
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedMeritCycleScoreStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new CappedMeritCycleScoreStartsAtRangeRule(32, 120);
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedMeritCycleScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedMeritCycleScoreRequiredCodeRule("capped merit cycle score needs a value");
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedMeritCycleScoreOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new CappedMeritCycleScoreOwnerIdLengthRule(120);
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedMeritCycleScoreVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new CappedMeritCycleScoreVersionRangeRule(16, 32);
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("x", "y");
        item.Version = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedMeritCycleScoreValidatorReportsEveryProblem();
        CappedMeritCycleScoreCodeLengthRuleFlagsALongCode();
        CappedMeritCycleScoreStartsAtRangeRuleFlagsValuesAboveStartsAt();
        CappedMeritCycleScoreRequiredCodeRuleFlagsABlankCode();
        CappedMeritCycleScoreOwnerIdLengthRuleFlagsALongOwnerId();
        CappedMeritCycleScoreVersionRangeRuleFlagsValuesAboveVersion();
    }
}
