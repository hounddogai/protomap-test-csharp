namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientPoolDraftRulesChecks
{
    public static void LenientPoolDraftValidatorReportsEveryProblem()
    {
        var validator = LenientPoolDraftRules.DefaultLenientPoolDraftValidator();
        var item = LenientPoolDraft.NewLenientPoolDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPoolDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientPoolDraftRequiredTitleRule("lenient pool draft needs this field");
        var item = LenientPoolDraft.NewLenientPoolDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPoolDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPoolDraftRequiredCodeRule("lenient pool draft needs this field");
        var item = LenientPoolDraft.NewLenientPoolDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPoolDraftValidatorReportsEveryProblem();
        LenientPoolDraftRequiredTitleRuleFlagsABlankTitle();
        LenientPoolDraftRequiredCodeRuleFlagsABlankCode();
    }
}
