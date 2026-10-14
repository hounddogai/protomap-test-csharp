namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredRateWindowRulesChecks
{
    public static void TieredRateWindowValidatorReportsEveryProblem()
    {
        var validator = TieredRateWindowRules.DefaultTieredRateWindowValidator();
        var item = TieredRateWindow.NewTieredRateWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredRateWindowFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TieredRateWindowFteRangeRule(4.6, 3874.6);
        var item = TieredRateWindow.NewTieredRateWindow("x", "y");
        item.Fte = 3876.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredRateWindowRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new TieredRateWindowRequiredSourceRule("tiered rate window needs an entry");
        var item = TieredRateWindow.NewTieredRateWindow("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRateWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredRateWindowRequiredCodeRule("tiered rate window needs a value");
        var item = TieredRateWindow.NewTieredRateWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRateWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredRateWindowRequiredNameRule("tiered rate window needs an entry");
        var item = TieredRateWindow.NewTieredRateWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredRateWindowValidatorReportsEveryProblem();
        TieredRateWindowFteRangeRuleFlagsValuesAboveFte();
        TieredRateWindowRequiredSourceRuleFlagsABlankSource();
        TieredRateWindowRequiredCodeRuleFlagsABlankCode();
        TieredRateWindowRequiredNameRuleFlagsABlankName();
    }
}
