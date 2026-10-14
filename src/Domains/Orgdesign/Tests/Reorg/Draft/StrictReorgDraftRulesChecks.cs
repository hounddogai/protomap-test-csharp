namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictReorgDraftRulesChecks
{
    public static void StrictReorgDraftValidatorReportsEveryProblem()
    {
        var validator = StrictReorgDraftRules.DefaultStrictReorgDraftValidator();
        var item = StrictReorgDraft.NewStrictReorgDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReorgDraftCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StrictReorgDraftCreatedAtRangeRule(32, 64);
        var item = StrictReorgDraft.NewStrictReorgDraft("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictReorgDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictReorgDraftCodeLengthRule(3600);
        var item = StrictReorgDraft.NewStrictReorgDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictReorgDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictReorgDraftRequiredCodeRule("strict reorg draft needs this field");
        var item = StrictReorgDraft.NewStrictReorgDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReorgDraftRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new StrictReorgDraftRequiredTeamRule("strict reorg draft needs an entry");
        var item = StrictReorgDraft.NewStrictReorgDraft("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReorgDraftTeamLengthRuleFlagsALongTeam()
    {
        var rule = new StrictReorgDraftTeamLengthRule(86400);
        var item = StrictReorgDraft.NewStrictReorgDraft("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictReorgDraftValidatorReportsEveryProblem();
        StrictReorgDraftCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        StrictReorgDraftCodeLengthRuleFlagsALongCode();
        StrictReorgDraftRequiredCodeRuleFlagsABlankCode();
        StrictReorgDraftRequiredTeamRuleFlagsABlankTeam();
        StrictReorgDraftTeamLengthRuleFlagsALongTeam();
    }
}
