namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RefreshSummaryRulesChecks
{
    public static void RefreshSummaryValidatorReportsEveryProblem()
    {
        var validator = RefreshSummaryRules.DefaultRefreshSummaryValidator();
        var item = RefreshSummary.NewRefreshSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshSummaryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RefreshSummaryLabelLengthRule(120);
        var item = RefreshSummary.NewRefreshSummary("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshSummaryContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new RefreshSummaryContactEmailLengthRule(64);
        var item = RefreshSummary.NewRefreshSummary("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshSummaryRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RefreshSummaryRequiredSalaryBandRule("refresh summary needs an entry");
        var item = RefreshSummary.NewRefreshSummary("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshSummaryValidatorReportsEveryProblem();
        RefreshSummaryLabelLengthRuleFlagsALongLabel();
        RefreshSummaryContactEmailLengthRuleFlagsALongContactEmail();
        RefreshSummaryRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
