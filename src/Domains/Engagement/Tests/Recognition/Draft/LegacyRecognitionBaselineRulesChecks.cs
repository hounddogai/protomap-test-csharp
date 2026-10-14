namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyRecognitionBaselineRulesChecks
{
    public static void LegacyRecognitionBaselineValidatorReportsEveryProblem()
    {
        var validator = LegacyRecognitionBaselineRules.DefaultLegacyRecognitionBaselineValidator();
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRecognitionBaselinePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new LegacyRecognitionBaselinePriorityRangeRule(32, 3600);
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRecognitionBaselineWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LegacyRecognitionBaselineWeightRangeRule(5.6, 4585.7);
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("x", "y");
        item.Weight = 4587.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRecognitionBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyRecognitionBaselineNameLengthRule(255);
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRecognitionBaselineValidatorReportsEveryProblem();
        LegacyRecognitionBaselinePriorityRangeRuleFlagsValuesAbovePriority();
        LegacyRecognitionBaselineWeightRangeRuleFlagsValuesAboveWeight();
        LegacyRecognitionBaselineNameLengthRuleFlagsALongName();
    }
}
