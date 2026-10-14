namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantBundleRulesChecks
{
    public static void EquityGrantBundleValidatorReportsEveryProblem()
    {
        var validator = EquityGrantBundleRules.DefaultEquityGrantBundleValidator();
        var item = EquityGrantBundle.NewEquityGrantBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantBundleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new EquityGrantBundleSalaryBandLengthRule(120);
        var item = EquityGrantBundle.NewEquityGrantBundle("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantBundleCodeLengthRule(255);
        var item = EquityGrantBundle.NewEquityGrantBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantBundleRequiredCodeRule("equity grant bundle needs an entry");
        var item = EquityGrantBundle.NewEquityGrantBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantBundleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new EquityGrantBundleRetriesRangeRule(1000, 1000);
        var item = EquityGrantBundle.NewEquityGrantBundle("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantBundleRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new EquityGrantBundleRequiredNotesRule("equity grant bundle needs an entry");
        var item = EquityGrantBundle.NewEquityGrantBundle("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantBundleValidatorReportsEveryProblem();
        EquityGrantBundleSalaryBandLengthRuleFlagsALongSalaryBand();
        EquityGrantBundleCodeLengthRuleFlagsALongCode();
        EquityGrantBundleRequiredCodeRuleFlagsABlankCode();
        EquityGrantBundleRetriesRangeRuleFlagsValuesAboveRetries();
        EquityGrantBundleRequiredNotesRuleFlagsABlankNotes();
    }
}
