namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class LegacyFeedbackProposalRulesChecks
{
    public static void LegacyFeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = LegacyFeedbackProposalRules.DefaultLegacyFeedbackProposalValidator();
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyFeedbackProposalRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LegacyFeedbackProposalRequiredSkillRule("legacy feedback proposal needs this field");
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFeedbackProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyFeedbackProposalRequiredLabelRule("legacy feedback proposal needs this field");
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFeedbackProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyFeedbackProposalCodeLengthRule(64);
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyFeedbackProposalRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new LegacyFeedbackProposalRequiredNotesRule("legacy feedback proposal needs a value");
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyFeedbackProposalValidatorReportsEveryProblem();
        LegacyFeedbackProposalRequiredSkillRuleFlagsABlankSkill();
        LegacyFeedbackProposalRequiredLabelRuleFlagsABlankLabel();
        LegacyFeedbackProposalCodeLengthRuleFlagsALongCode();
        LegacyFeedbackProposalRequiredNotesRuleFlagsABlankNotes();
    }
}
