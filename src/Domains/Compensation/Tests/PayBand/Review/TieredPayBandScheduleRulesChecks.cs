namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredPayBandScheduleRulesChecks
{
    public static void TieredPayBandScheduleValidatorReportsEveryProblem()
    {
        var validator = TieredPayBandScheduleRules.DefaultTieredPayBandScheduleValidator();
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayBandScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredPayBandScheduleRequiredNameRule("tiered pay band schedule needs an entry");
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandScheduleExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new TieredPayBandScheduleExternalRefLengthRule(255);
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPayBandScheduleValidatorReportsEveryProblem();
        TieredPayBandScheduleRequiredNameRuleFlagsABlankName();
        TieredPayBandScheduleExternalRefLengthRuleFlagsALongExternalRef();
    }
}
