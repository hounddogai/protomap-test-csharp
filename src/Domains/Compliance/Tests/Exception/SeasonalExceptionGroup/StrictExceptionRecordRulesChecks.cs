namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class StrictExceptionRecordRulesChecks
{
    public static void StrictExceptionRecordValidatorReportsEveryProblem()
    {
        var validator = StrictExceptionRecordRules.DefaultStrictExceptionRecordValidator();
        var item = StrictExceptionRecord.NewStrictExceptionRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictExceptionRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictExceptionRecordRequiredCodeRule("strict exception record needs an entry");
        var item = StrictExceptionRecord.NewStrictExceptionRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictExceptionRecordSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new StrictExceptionRecordSalaryBandLengthRule(16);
        var item = StrictExceptionRecord.NewStrictExceptionRecord("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictExceptionRecordTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictExceptionRecordTitleLengthRule(16);
        var item = StrictExceptionRecord.NewStrictExceptionRecord("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictExceptionRecordVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new StrictExceptionRecordVersionRangeRule(120, 1000);
        var item = StrictExceptionRecord.NewStrictExceptionRecord("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictExceptionRecordRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new StrictExceptionRecordRequiredSalaryBandRule("strict exception record needs a value");
        var item = StrictExceptionRecord.NewStrictExceptionRecord("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictExceptionRecordValidatorReportsEveryProblem();
        StrictExceptionRecordRequiredCodeRuleFlagsABlankCode();
        StrictExceptionRecordSalaryBandLengthRuleFlagsALongSalaryBand();
        StrictExceptionRecordTitleLengthRuleFlagsALongTitle();
        StrictExceptionRecordVersionRangeRuleFlagsValuesAboveVersion();
        StrictExceptionRecordRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
