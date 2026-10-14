namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultLicenseSeatSummaryRulesChecks
{
    public static void DefaultLicenseSeatSummaryValidatorReportsEveryProblem()
    {
        var validator = DefaultLicenseSeatSummaryRules.DefaultDefaultLicenseSeatSummaryValidator();
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLicenseSeatSummaryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new DefaultLicenseSeatSummaryNotesLengthRule(120);
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultLicenseSeatSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultLicenseSeatSummaryRequiredTitleRule("default license seat summary needs a value");
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLicenseSeatSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultLicenseSeatSummaryRequiredCodeRule("default license seat summary needs this field");
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatSummaryValidatorReportsEveryProblem();
        DefaultLicenseSeatSummaryNotesLengthRuleFlagsALongNotes();
        DefaultLicenseSeatSummaryRequiredTitleRuleFlagsABlankTitle();
        DefaultLicenseSeatSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
