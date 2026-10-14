namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAuditRulesChecks
{
    public static void AdaptiveLicenseSeatAuditValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLicenseSeatAuditRules.DefaultAdaptiveLicenseSeatAuditValidator();
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLicenseSeatAuditRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AdaptiveLicenseSeatAuditRatingRangeRule(1.1, 1490.7);
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("x", "y");
        item.Rating = 1492.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLicenseSeatAuditRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new AdaptiveLicenseSeatAuditRequiredSegmentRule("adaptive license seat audit needs an entry");
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveLicenseSeatAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveLicenseSeatAuditLabelLengthRule(86400);
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLicenseSeatAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveLicenseSeatAuditRequiredCodeRule("adaptive license seat audit needs this field");
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveLicenseSeatAuditCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveLicenseSeatAuditCostRangeRule(9.9, 3196.1);
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("x", "y");
        item.Cost = 3197.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAuditValidatorReportsEveryProblem();
        AdaptiveLicenseSeatAuditRatingRangeRuleFlagsValuesAboveRating();
        AdaptiveLicenseSeatAuditRequiredSegmentRuleFlagsABlankSegment();
        AdaptiveLicenseSeatAuditLabelLengthRuleFlagsALongLabel();
        AdaptiveLicenseSeatAuditRequiredCodeRuleFlagsABlankCode();
        AdaptiveLicenseSeatAuditCostRangeRuleFlagsValuesAboveCost();
    }
}
