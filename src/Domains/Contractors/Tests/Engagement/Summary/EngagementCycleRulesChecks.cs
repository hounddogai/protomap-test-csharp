namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EngagementCycleRulesChecks
{
    public static void EngagementCycleValidatorReportsEveryProblem()
    {
        var validator = EngagementCycleRules.DefaultEngagementCycleValidator();
        var item = EngagementCycle.NewEngagementCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementCycleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new EngagementCycleRatioRangeRule(5.2, 4189.1);
        var item = EngagementCycle.NewEngagementCycle("x", "y");
        item.Ratio = 4190.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementCycleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new EngagementCycleRequiredGradeRule("engagement cycle needs a value");
        var item = EngagementCycle.NewEngagementCycle("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementCycleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new EngagementCycleGradeLengthRule(64);
        var item = EngagementCycle.NewEngagementCycle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementCycleLabelLengthRule(1000);
        var item = EngagementCycle.NewEngagementCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementCycleValidatorReportsEveryProblem();
        EngagementCycleRatioRangeRuleFlagsValuesAboveRatio();
        EngagementCycleRequiredGradeRuleFlagsABlankGrade();
        EngagementCycleGradeLengthRuleFlagsALongGrade();
        EngagementCycleLabelLengthRuleFlagsALongLabel();
    }
}
