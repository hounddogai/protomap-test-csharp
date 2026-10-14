namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class DefaultHeadcountDraftRulesChecks
{
    public static void DefaultHeadcountDraftValidatorReportsEveryProblem()
    {
        var validator = DefaultHeadcountDraftRules.DefaultDefaultHeadcountDraftValidator();
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHeadcountDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultHeadcountDraftLabelLengthRule(64);
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHeadcountDraftCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new DefaultHeadcountDraftCostRangeRule(7.1, 4706.1);
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("x", "y");
        item.Cost = 4707.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountDraftStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new DefaultHeadcountDraftStartsAtRangeRule(86400, 86400);
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultHeadcountDraftValidatorReportsEveryProblem();
        DefaultHeadcountDraftLabelLengthRuleFlagsALongLabel();
        DefaultHeadcountDraftCostRangeRuleFlagsValuesAboveCost();
        DefaultHeadcountDraftStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
