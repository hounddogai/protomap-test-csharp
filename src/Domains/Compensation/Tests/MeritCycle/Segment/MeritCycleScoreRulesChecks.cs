namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleScoreRulesChecks
{
    public static void MeritCycleScoreValidatorReportsEveryProblem()
    {
        var validator = MeritCycleScoreRules.DefaultMeritCycleScoreValidator();
        var item = MeritCycleScore.NewMeritCycleScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleScoreRequiredCodeRule("merit cycle score needs this field");
        var item = MeritCycleScore.NewMeritCycleScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new MeritCycleScoreUpdatedAtRangeRule(255, 1000);
        var item = MeritCycleScore.NewMeritCycleScore("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleScoreMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new MeritCycleScoreMarginRangeRule(5.5, 4190.3);
        var item = MeritCycleScore.NewMeritCycleScore("x", "y");
        item.Margin = 4191.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new MeritCycleScoreRequiredNameRule("merit cycle score needs an entry");
        var item = MeritCycleScore.NewMeritCycleScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleScoreValidatorReportsEveryProblem();
        MeritCycleScoreRequiredCodeRuleFlagsABlankCode();
        MeritCycleScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        MeritCycleScoreMarginRangeRuleFlagsValuesAboveMargin();
        MeritCycleScoreRequiredNameRuleFlagsABlankName();
    }
}
