namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class RegionalSkillProfileManifestChecks
{
    public static void RegionalSkillProfileManifestStartsInIdle()
    {
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("code-201", "name-770");
        Check.Equal(item.Status, RegionalSkillProfileManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSkillProfileManifestMovesToReady()
    {
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("code-201", "name-770");
        try
        {
            item.MoveTo(RegionalSkillProfileManifestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSkillProfileManifestStatus.Ready, "status");
    }

    public static void RegionalSkillProfileManifestRejectsMovesFromStopped()
    {
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("code-201", "name-770");
        item.Status = RegionalSkillProfileManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalSkillProfileManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalSkillProfileManifestStartsInIdle();
        RegionalSkillProfileManifestMovesToReady();
        RegionalSkillProfileManifestRejectsMovesFromStopped();
    }
}
