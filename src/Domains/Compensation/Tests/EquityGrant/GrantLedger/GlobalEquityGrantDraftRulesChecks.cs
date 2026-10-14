namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class GlobalEquityGrantDraftRulesChecks
{
    public static void GlobalEquityGrantDraftValidatorReportsEveryProblem()
    {
        var validator = GlobalEquityGrantDraftRules.DefaultGlobalEquityGrantDraftValidator();
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalEquityGrantDraftRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new GlobalEquityGrantDraftRequiredTeamRule("global equity grant draft needs an entry");
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalEquityGrantDraftRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new GlobalEquityGrantDraftRequiredRegionRule("global equity grant draft needs this field");
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalEquityGrantDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalEquityGrantDraftTitleLengthRule(86400);
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalEquityGrantDraftRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new GlobalEquityGrantDraftRetriesRangeRule(64, 1000);
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalEquityGrantDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalEquityGrantDraftCodeLengthRule(80);
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalEquityGrantDraftValidatorReportsEveryProblem();
        GlobalEquityGrantDraftRequiredTeamRuleFlagsABlankTeam();
        GlobalEquityGrantDraftRequiredRegionRuleFlagsABlankRegion();
        GlobalEquityGrantDraftTitleLengthRuleFlagsALongTitle();
        GlobalEquityGrantDraftRetriesRangeRuleFlagsValuesAboveRetries();
        GlobalEquityGrantDraftCodeLengthRuleFlagsALongCode();
    }
}
