namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredSkillProfileDigestChecks
{
    public static void TieredSkillProfileDigestStartsInNew()
    {
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("code-76", "name-798");
        Check.Equal(item.Status, TieredSkillProfileDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSkillProfileDigestMovesToFailed()
    {
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("code-76", "name-798");
        try
        {
            item.MoveTo(TieredSkillProfileDigestStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSkillProfileDigestStatus.Failed, "status");
    }

    public static void TieredSkillProfileDigestRejectsMovesFromFailed()
    {
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("code-76", "name-798");
        item.Status = TieredSkillProfileDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredSkillProfileDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredSkillProfileDigestStartsInNew();
        TieredSkillProfileDigestMovesToFailed();
        TieredSkillProfileDigestRejectsMovesFromFailed();
    }
}
