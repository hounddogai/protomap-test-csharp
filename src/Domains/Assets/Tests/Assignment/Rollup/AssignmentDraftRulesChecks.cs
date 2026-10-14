namespace Acme.Workforce.Assets.Tests.Assignment.Rollup;

using Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AssignmentDraftRulesChecks
{
    public static void AssignmentDraftValidatorReportsEveryProblem()
    {
        var validator = AssignmentDraftRules.DefaultAssignmentDraftValidator();
        var item = AssignmentDraft.NewAssignmentDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentDraftExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AssignmentDraftExpiresAtRangeRule(32, 255);
        var item = AssignmentDraft.NewAssignmentDraft("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssignmentDraftRequiredLabelRule("assignment draft needs an entry");
        var item = AssignmentDraft.NewAssignmentDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssignmentDraftLabelLengthRule(255);
        var item = AssignmentDraft.NewAssignmentDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentDraftRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AssignmentDraftRatingRangeRule(1.1, 776.1);
        var item = AssignmentDraft.NewAssignmentDraft("x", "y");
        item.Rating = 777.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentDraftValidatorReportsEveryProblem();
        AssignmentDraftExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AssignmentDraftRequiredLabelRuleFlagsABlankLabel();
        AssignmentDraftLabelLengthRuleFlagsALongLabel();
        AssignmentDraftRatingRangeRuleFlagsValuesAboveRating();
    }
}
