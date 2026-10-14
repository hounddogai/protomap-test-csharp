namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PrimarySkillProfileManifestChecks
{
    public static void PrimarySkillProfileManifestStartsInDraft()
    {
        var item = PrimarySkillProfileManifest.NewPrimarySkillProfileManifest("code-810", "label-171");
        Check.Equal(item.Status, PrimarySkillProfileManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySkillProfileManifestMovesToPending()
    {
        var item = PrimarySkillProfileManifest.NewPrimarySkillProfileManifest("code-810", "label-171");
        try
        {
            item.MoveTo(PrimarySkillProfileManifestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySkillProfileManifestStatus.Pending, "status");
    }

    public static void PrimarySkillProfileManifestRejectsMovesFromClosed()
    {
        var item = PrimarySkillProfileManifest.NewPrimarySkillProfileManifest("code-810", "label-171");
        item.Status = PrimarySkillProfileManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimarySkillProfileManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimarySkillProfileManifestStartsInDraft();
        PrimarySkillProfileManifestMovesToPending();
        PrimarySkillProfileManifestRejectsMovesFromClosed();
    }
}
