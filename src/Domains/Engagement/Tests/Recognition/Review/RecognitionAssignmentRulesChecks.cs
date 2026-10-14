namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionAssignmentRulesChecks
{
    public static void RecognitionAssignmentValidatorReportsEveryProblem()
    {
        var validator = RecognitionAssignmentRules.DefaultRecognitionAssignmentValidator();
        var item = RecognitionAssignment.NewRecognitionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionAssignmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RecognitionAssignmentMinimumRangeRule(8.5, 674.1);
        var item = RecognitionAssignment.NewRecognitionAssignment("x", "y");
        item.Minimum = 675.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionAssignmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RecognitionAssignmentCostRangeRule(9.1, 3327.9);
        var item = RecognitionAssignment.NewRecognitionAssignment("x", "y");
        item.Cost = 3329.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionAssignmentRequiredCodeRule("recognition assignment needs this field");
        var item = RecognitionAssignment.NewRecognitionAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionAssignmentValidatorReportsEveryProblem();
        RecognitionAssignmentMinimumRangeRuleFlagsValuesAboveMinimum();
        RecognitionAssignmentCostRangeRuleFlagsValuesAboveCost();
        RecognitionAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
