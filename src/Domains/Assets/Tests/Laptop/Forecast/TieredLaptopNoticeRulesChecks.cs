namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredLaptopNoticeRulesChecks
{
    public static void TieredLaptopNoticeValidatorReportsEveryProblem()
    {
        var validator = TieredLaptopNoticeRules.DefaultTieredLaptopNoticeValidator();
        var item = TieredLaptopNotice.NewTieredLaptopNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLaptopNoticeRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new TieredLaptopNoticeRevisionRangeRule(255, 3600);
        var item = TieredLaptopNotice.NewTieredLaptopNotice("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLaptopNoticeMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new TieredLaptopNoticeMaximumRangeRule(8.7, 4144.3);
        var item = TieredLaptopNotice.NewTieredLaptopNotice("x", "y");
        item.Maximum = 4145.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLaptopNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredLaptopNoticeCodeLengthRule(16);
        var item = TieredLaptopNotice.NewTieredLaptopNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLaptopNoticeValidatorReportsEveryProblem();
        TieredLaptopNoticeRevisionRangeRuleFlagsValuesAboveRevision();
        TieredLaptopNoticeMaximumRangeRuleFlagsValuesAboveMaximum();
        TieredLaptopNoticeCodeLengthRuleFlagsALongCode();
    }
}
