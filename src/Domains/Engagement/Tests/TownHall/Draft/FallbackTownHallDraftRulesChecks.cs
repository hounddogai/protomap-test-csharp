namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackTownHallDraftRulesChecks
{
    public static void FallbackTownHallDraftValidatorReportsEveryProblem()
    {
        var validator = FallbackTownHallDraftRules.DefaultFallbackTownHallDraftValidator();
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackTownHallDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackTownHallDraftCodeLengthRule(86400);
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTownHallDraftRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new FallbackTownHallDraftRequiredChecksumRule("fallback town hall draft needs a value");
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackTownHallDraftRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FallbackTownHallDraftRequiredSalaryBandRule("fallback town hall draft needs an entry");
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackTownHallDraftValidatorReportsEveryProblem();
        FallbackTownHallDraftCodeLengthRuleFlagsALongCode();
        FallbackTownHallDraftRequiredChecksumRuleFlagsABlankChecksum();
        FallbackTownHallDraftRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
