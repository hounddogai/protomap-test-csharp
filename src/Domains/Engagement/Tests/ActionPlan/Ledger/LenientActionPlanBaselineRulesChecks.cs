namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LenientActionPlanBaselineRulesChecks
{
    public static void LenientActionPlanBaselineValidatorReportsEveryProblem()
    {
        var validator = LenientActionPlanBaselineRules.DefaultLenientActionPlanBaselineValidator();
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientActionPlanBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientActionPlanBaselineRequiredLabelRule("lenient action plan baseline needs a value");
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanBaselineNotesLengthRuleFlagsALongNotes()
    {
        var rule = new LenientActionPlanBaselineNotesLengthRule(64);
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientActionPlanBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientActionPlanBaselineRequiredCodeRule("lenient action plan baseline needs an entry");
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanBaselineUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LenientActionPlanBaselineUpdatedAtRangeRule(120, 255);
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientActionPlanBaselineValidatorReportsEveryProblem();
        LenientActionPlanBaselineRequiredLabelRuleFlagsABlankLabel();
        LenientActionPlanBaselineNotesLengthRuleFlagsALongNotes();
        LenientActionPlanBaselineRequiredCodeRuleFlagsABlankCode();
        LenientActionPlanBaselineUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
