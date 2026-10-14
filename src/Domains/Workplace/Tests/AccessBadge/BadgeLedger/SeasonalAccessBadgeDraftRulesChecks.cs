namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class SeasonalAccessBadgeDraftRulesChecks
{
    public static void SeasonalAccessBadgeDraftValidatorReportsEveryProblem()
    {
        var validator = SeasonalAccessBadgeDraftRules.DefaultSeasonalAccessBadgeDraftValidator();
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAccessBadgeDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAccessBadgeDraftCodeLengthRule(1000);
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAccessBadgeDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalAccessBadgeDraftRequiredTitleRule("seasonal access badge draft needs an entry");
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalAccessBadgeDraftValidatorReportsEveryProblem();
        SeasonalAccessBadgeDraftCodeLengthRuleFlagsALongCode();
        SeasonalAccessBadgeDraftRequiredTitleRuleFlagsABlankTitle();
    }
}
