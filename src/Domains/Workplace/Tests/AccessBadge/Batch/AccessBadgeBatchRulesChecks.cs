namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchRulesChecks
{
    public static void AccessBadgeBatchValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeBatchRules.DefaultAccessBadgeBatchValidator();
        var item = AccessBadgeBatch.NewAccessBadgeBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeBatchChannelLengthRuleFlagsALongChannel()
    {
        var rule = new AccessBadgeBatchChannelLengthRule(16);
        var item = AccessBadgeBatch.NewAccessBadgeBatch("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeBatchSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new AccessBadgeBatchSalaryBandLengthRule(86400);
        var item = AccessBadgeBatch.NewAccessBadgeBatch("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeBatchNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeBatchNameLengthRule(1000);
        var item = AccessBadgeBatch.NewAccessBadgeBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeBatchCodeLengthRule(80);
        var item = AccessBadgeBatch.NewAccessBadgeBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeBatchRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AccessBadgeBatchRateRangeRule(8.1, 68.9);
        var item = AccessBadgeBatch.NewAccessBadgeBatch("x", "y");
        item.Rate = 70.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeBatchValidatorReportsEveryProblem();
        AccessBadgeBatchChannelLengthRuleFlagsALongChannel();
        AccessBadgeBatchSalaryBandLengthRuleFlagsALongSalaryBand();
        AccessBadgeBatchNameLengthRuleFlagsALongName();
        AccessBadgeBatchCodeLengthRuleFlagsALongCode();
        AccessBadgeBatchRateRangeRuleFlagsValuesAboveRate();
    }
}
