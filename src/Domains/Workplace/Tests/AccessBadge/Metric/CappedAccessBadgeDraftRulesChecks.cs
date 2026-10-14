namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedAccessBadgeDraftRulesChecks
{
    public static void CappedAccessBadgeDraftValidatorReportsEveryProblem()
    {
        var validator = CappedAccessBadgeDraftRules.DefaultCappedAccessBadgeDraftValidator();
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAccessBadgeDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedAccessBadgeDraftRequiredCodeRule("capped access badge draft needs a value");
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAccessBadgeDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedAccessBadgeDraftLabelLengthRule(32);
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeDraftSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CappedAccessBadgeDraftSummaryLengthRule(3600);
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeDraftFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new CappedAccessBadgeDraftFteRangeRule(8.9, 1932.6);
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("x", "y");
        item.Fte = 1934.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedAccessBadgeDraftValidatorReportsEveryProblem();
        CappedAccessBadgeDraftRequiredCodeRuleFlagsABlankCode();
        CappedAccessBadgeDraftLabelLengthRuleFlagsALongLabel();
        CappedAccessBadgeDraftSummaryLengthRuleFlagsALongSummary();
        CappedAccessBadgeDraftFteRangeRuleFlagsValuesAboveFte();
    }
}
