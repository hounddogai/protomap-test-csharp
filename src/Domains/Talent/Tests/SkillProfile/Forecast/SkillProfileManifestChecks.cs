namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileManifestChecks
{
    public static void SkillProfileManifestStartsInRequested()
    {
        var item = SkillProfileManifest.NewSkillProfileManifest("code-372", "label-72");
        Check.Equal(item.Status, SkillProfileManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileManifestMovesToExpired()
    {
        var item = SkillProfileManifest.NewSkillProfileManifest("code-372", "label-72");
        try
        {
            item.MoveTo(SkillProfileManifestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileManifestStatus.Expired, "status");
    }

    public static void SkillProfileManifestRejectsMovesFromExpired()
    {
        var item = SkillProfileManifest.NewSkillProfileManifest("code-372", "label-72");
        item.Status = SkillProfileManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(SkillProfileManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SkillProfileManifestStartsInRequested();
        SkillProfileManifestMovesToExpired();
        SkillProfileManifestRejectsMovesFromExpired();
    }
}
