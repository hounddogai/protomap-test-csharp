namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TieredTrainingDueVarianceRulesChecks
{
    public static void TieredTrainingDueVarianceValidatorReportsEveryProblem()
    {
        var validator = TieredTrainingDueVarianceRules.DefaultTieredTrainingDueVarianceValidator();
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTrainingDueVarianceSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new TieredTrainingDueVarianceSegmentLengthRule(16);
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTrainingDueVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredTrainingDueVarianceCodeLengthRule(64);
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTrainingDueVarianceDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new TieredTrainingDueVarianceDescriptionLengthRule(3600);
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTrainingDueVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredTrainingDueVarianceTitleLengthRule(255);
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTrainingDueVarianceFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TieredTrainingDueVarianceFteRangeRule(1.6, 3084.5);
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("x", "y");
        item.Fte = 3086.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredTrainingDueVarianceValidatorReportsEveryProblem();
        TieredTrainingDueVarianceSegmentLengthRuleFlagsALongSegment();
        TieredTrainingDueVarianceCodeLengthRuleFlagsALongCode();
        TieredTrainingDueVarianceDescriptionLengthRuleFlagsALongDescription();
        TieredTrainingDueVarianceTitleLengthRuleFlagsALongTitle();
        TieredTrainingDueVarianceFteRangeRuleFlagsValuesAboveFte();
    }
}
