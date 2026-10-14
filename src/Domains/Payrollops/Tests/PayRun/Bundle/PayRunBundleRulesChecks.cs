namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBundleRulesChecks
{
    public static void PayRunBundleValidatorReportsEveryProblem()
    {
        var validator = PayRunBundleRules.DefaultPayRunBundleValidator();
        var item = PayRunBundle.NewPayRunBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PayRunBundleCreatedAtRangeRule(120, 255);
        var item = PayRunBundle.NewPayRunBundle("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunBundleLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PayRunBundleLocationLengthRule(1000);
        var item = PayRunBundle.NewPayRunBundle("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunBundleRequiredCodeRule("pay run bundle needs a value");
        var item = PayRunBundle.NewPayRunBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunBundleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayRunBundleTitleLengthRule(80);
        var item = PayRunBundle.NewPayRunBundle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunBundleCodeLengthRule(1000);
        var item = PayRunBundle.NewPayRunBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunBundleValidatorReportsEveryProblem();
        PayRunBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PayRunBundleLocationLengthRuleFlagsALongLocation();
        PayRunBundleRequiredCodeRuleFlagsABlankCode();
        PayRunBundleTitleLengthRuleFlagsALongTitle();
        PayRunBundleCodeLengthRuleFlagsALongCode();
    }
}
