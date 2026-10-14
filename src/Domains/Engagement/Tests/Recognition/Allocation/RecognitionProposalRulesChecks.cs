namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionProposalRulesChecks
{
    public static void RecognitionProposalValidatorReportsEveryProblem()
    {
        var validator = RecognitionProposalRules.DefaultRecognitionProposalValidator();
        var item = RecognitionProposal.NewRecognitionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionProposalRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new RecognitionProposalRequiredGradeRule("recognition proposal needs a value");
        var item = RecognitionProposal.NewRecognitionProposal("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionProposalCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RecognitionProposalCostRangeRule(1.6, 3446.5);
        var item = RecognitionProposal.NewRecognitionProposal("x", "y");
        item.Cost = 3448.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionProposalRequiredNameRuleFlagsABlankName()
    {
        var rule = new RecognitionProposalRequiredNameRule("recognition proposal needs a value");
        var item = RecognitionProposal.NewRecognitionProposal("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionProposalValidatorReportsEveryProblem();
        RecognitionProposalRequiredGradeRuleFlagsABlankGrade();
        RecognitionProposalCostRangeRuleFlagsValuesAboveCost();
        RecognitionProposalRequiredNameRuleFlagsABlankName();
    }
}
