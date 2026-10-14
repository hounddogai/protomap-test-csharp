namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientCareerPathRuleChecks
{
    public static void LenientCareerPathRuleStartsInOpen()
    {
        var item = LenientCareerPathRule.NewLenientCareerPathRule("code-837", "name-685");
        Check.Equal(item.Status, LenientCareerPathRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientCareerPathRuleMovesToResolved()
    {
        var item = LenientCareerPathRule.NewLenientCareerPathRule("code-837", "name-685");
        try
        {
            item.MoveTo(LenientCareerPathRuleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientCareerPathRuleStatus.Resolved, "status");
    }

    public static void LenientCareerPathRuleRejectsMovesFromArchived()
    {
        var item = LenientCareerPathRule.NewLenientCareerPathRule("code-837", "name-685");
        item.Status = LenientCareerPathRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientCareerPathRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientCareerPathRuleStartsInOpen();
        LenientCareerPathRuleMovesToResolved();
        LenientCareerPathRuleRejectsMovesFromArchived();
    }
}
