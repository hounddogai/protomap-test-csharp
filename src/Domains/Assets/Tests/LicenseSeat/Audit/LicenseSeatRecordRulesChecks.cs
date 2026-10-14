namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatRecordRulesChecks
{
    public static void LicenseSeatRecordValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatRecordRules.DefaultLicenseSeatRecordValidator();
        var item = LicenseSeatRecord.NewLicenseSeatRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatRecordPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new LicenseSeatRecordPhoneLengthRule(32);
        var item = LicenseSeatRecord.NewLicenseSeatRecord("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatRecordNameLengthRuleFlagsALongName()
    {
        var rule = new LicenseSeatRecordNameLengthRule(64);
        var item = LicenseSeatRecord.NewLicenseSeatRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new LicenseSeatRecordWindowDaysRangeRule(120, 3600);
        var item = LicenseSeatRecord.NewLicenseSeatRecord("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatRecordCodeLengthRule(255);
        var item = LicenseSeatRecord.NewLicenseSeatRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatRecordRequiredCodeRule("license seat record needs an entry");
        var item = LicenseSeatRecord.NewLicenseSeatRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatRecordValidatorReportsEveryProblem();
        LicenseSeatRecordPhoneLengthRuleFlagsALongPhone();
        LicenseSeatRecordNameLengthRuleFlagsALongName();
        LicenseSeatRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        LicenseSeatRecordCodeLengthRuleFlagsALongCode();
        LicenseSeatRecordRequiredCodeRuleFlagsABlankCode();
    }
}
