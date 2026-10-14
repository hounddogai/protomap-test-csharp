namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationMappingChecks
{
    public static void NominationMappingStartsInRequested()
    {
        var item = NominationMapping.NewNominationMapping("code-742", "name-237");
        Check.Equal(item.Status, NominationMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationMappingMovesToRevoked()
    {
        var item = NominationMapping.NewNominationMapping("code-742", "name-237");
        try
        {
            item.MoveTo(NominationMappingStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationMappingStatus.Revoked, "status");
    }

    public static void NominationMappingRejectsMovesFromExpired()
    {
        var item = NominationMapping.NewNominationMapping("code-742", "name-237");
        item.Status = NominationMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(NominationMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        NominationMappingStartsInRequested();
        NominationMappingMovesToRevoked();
        NominationMappingRejectsMovesFromExpired();
    }
}
