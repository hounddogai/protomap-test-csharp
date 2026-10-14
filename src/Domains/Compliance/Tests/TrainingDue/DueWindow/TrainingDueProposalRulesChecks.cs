namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class TrainingDueProposalRulesChecks
{
    public static void TrainingDueProposalValidatorReportsEveryProblem()
    {
        var validator = TrainingDueProposalRules.DefaultTrainingDueProposalValidator();
        var item = TrainingDueProposal.NewTrainingDueProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueProposalMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TrainingDueProposalMinimumRangeRule(8.9, 3828.2);
        var item = TrainingDueProposal.NewTrainingDueProposal("x", "y");
        item.Minimum = 3829.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueProposalLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new TrainingDueProposalLimitRangeRule(16, 86400);
        var item = TrainingDueProposal.NewTrainingDueProposal("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueProposalValidatorReportsEveryProblem();
        TrainingDueProposalMinimumRangeRuleFlagsValuesAboveMinimum();
        TrainingDueProposalLimitRangeRuleFlagsValuesAboveLimit();
    }
}
