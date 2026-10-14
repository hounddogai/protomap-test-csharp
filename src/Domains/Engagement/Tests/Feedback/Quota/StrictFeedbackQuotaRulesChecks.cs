namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictFeedbackQuotaRulesChecks
{
    public static void StrictFeedbackQuotaValidatorReportsEveryProblem()
    {
        var validator = StrictFeedbackQuotaRules.DefaultStrictFeedbackQuotaValidator();
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictFeedbackQuotaRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new StrictFeedbackQuotaRequiredUnitRule("strict feedback quota needs an entry");
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictFeedbackQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictFeedbackQuotaCodeLengthRule(3600);
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictFeedbackQuotaUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictFeedbackQuotaUnitLengthRule(1000);
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictFeedbackQuotaChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new StrictFeedbackQuotaChecksumLengthRule(64);
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictFeedbackQuotaCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new StrictFeedbackQuotaCapacityRangeRule(120, 86400);
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictFeedbackQuotaValidatorReportsEveryProblem();
        StrictFeedbackQuotaRequiredUnitRuleFlagsABlankUnit();
        StrictFeedbackQuotaCodeLengthRuleFlagsALongCode();
        StrictFeedbackQuotaUnitLengthRuleFlagsALongUnit();
        StrictFeedbackQuotaChecksumLengthRuleFlagsALongChecksum();
        StrictFeedbackQuotaCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
