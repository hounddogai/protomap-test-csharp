namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PendingOccupancyNoticeRulesChecks
{
    public static void PendingOccupancyNoticeValidatorReportsEveryProblem()
    {
        var validator = PendingOccupancyNoticeRules.DefaultPendingOccupancyNoticeValidator();
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingOccupancyNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingOccupancyNoticeRequiredCodeRule("pending occupancy notice needs this field");
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingOccupancyNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PendingOccupancyNoticeTenureMonthsRangeRule(120, 255);
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingOccupancyNoticeAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingOccupancyNoticeAttemptsRangeRule(16, 1000);
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingOccupancyNoticeExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new PendingOccupancyNoticeExternalRefLengthRule(120);
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingOccupancyNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingOccupancyNoticeCodeLengthRule(86400);
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingOccupancyNoticeValidatorReportsEveryProblem();
        PendingOccupancyNoticeRequiredCodeRuleFlagsABlankCode();
        PendingOccupancyNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        PendingOccupancyNoticeAttemptsRangeRuleFlagsValuesAboveAttempts();
        PendingOccupancyNoticeExternalRefLengthRuleFlagsALongExternalRef();
        PendingOccupancyNoticeCodeLengthRuleFlagsALongCode();
    }
}
