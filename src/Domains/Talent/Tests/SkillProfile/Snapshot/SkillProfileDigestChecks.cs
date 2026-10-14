namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileDigestChecks
{
    public static void SkillProfileDigestStartsInOpen()
    {
        var item = SkillProfileDigest.NewSkillProfileDigest("code-747", "title-935");
        Check.Equal(item.Status, SkillProfileDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileDigestMovesToResolved()
    {
        var item = SkillProfileDigest.NewSkillProfileDigest("code-747", "title-935");
        try
        {
            item.MoveTo(SkillProfileDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileDigestStatus.Resolved, "status");
    }

    public static void SkillProfileDigestRejectsMovesFromArchived()
    {
        var item = SkillProfileDigest.NewSkillProfileDigest("code-747", "title-935");
        item.Status = SkillProfileDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(SkillProfileDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        SkillProfileDigestStartsInOpen();
        SkillProfileDigestMovesToResolved();
        SkillProfileDigestRejectsMovesFromArchived();
    }
}
