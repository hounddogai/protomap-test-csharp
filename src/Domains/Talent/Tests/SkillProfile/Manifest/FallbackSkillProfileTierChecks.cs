namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackSkillProfileTierChecks
{
    public static void FallbackSkillProfileTierStartsInNew()
    {
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("code-567", "name-193");
        Check.Equal(item.Status, FallbackSkillProfileTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSkillProfileTierMovesToQueued()
    {
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("code-567", "name-193");
        try
        {
            item.MoveTo(FallbackSkillProfileTierStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSkillProfileTierStatus.Queued, "status");
    }

    public static void FallbackSkillProfileTierRejectsMovesFromFailed()
    {
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("code-567", "name-193");
        item.Status = FallbackSkillProfileTierStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackSkillProfileTierStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackSkillProfileTierStartsInNew();
        FallbackSkillProfileTierMovesToQueued();
        FallbackSkillProfileTierRejectsMovesFromFailed();
    }
}
