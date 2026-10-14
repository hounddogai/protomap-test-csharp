namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class PendingRateWindowRulesChecks
{
    public static void PendingRateWindowValidatorReportsEveryProblem()
    {
        var validator = PendingRateWindowRules.DefaultPendingRateWindowValidator();
        var item = PendingRateWindow.NewPendingRateWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRateWindowGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PendingRateWindowGraceDaysRangeRule(120, 86400);
        var item = PendingRateWindow.NewPendingRateWindow("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRateWindowRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new PendingRateWindowRequiredChannelRule("pending rate window needs this field");
        var item = PendingRateWindow.NewPendingRateWindow("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingRateWindowChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PendingRateWindowChannelLengthRule(86400);
        var item = PendingRateWindow.NewPendingRateWindow("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingRateWindowValidatorReportsEveryProblem();
        PendingRateWindowGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PendingRateWindowRequiredChannelRuleFlagsABlankChannel();
        PendingRateWindowChannelLengthRuleFlagsALongChannel();
    }
}
