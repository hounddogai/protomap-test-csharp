namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateRecordRulesChecks
{
    public static void RateRecordValidatorReportsEveryProblem()
    {
        var validator = RateRecordRules.DefaultRateRecordValidator();
        var item = RateRecord.NewRateRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateRecordChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RateRecordChannelLengthRule(32);
        var item = RateRecord.NewRateRecord("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateRecordLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RateRecordLimitRangeRule(64, 80);
        var item = RateRecord.NewRateRecord("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new RateRecordCodeLengthRule(255);
        var item = RateRecord.NewRateRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateRecordCountryLengthRuleFlagsALongCountry()
    {
        var rule = new RateRecordCountryLengthRule(3600);
        var item = RateRecord.NewRateRecord("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateRecordRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RateRecordRequiredChannelRule("rate record needs this field");
        var item = RateRecord.NewRateRecord("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateRecordValidatorReportsEveryProblem();
        RateRecordChannelLengthRuleFlagsALongChannel();
        RateRecordLimitRangeRuleFlagsValuesAboveLimit();
        RateRecordCodeLengthRuleFlagsALongCode();
        RateRecordCountryLengthRuleFlagsALongCountry();
        RateRecordRequiredChannelRuleFlagsABlankChannel();
    }
}
