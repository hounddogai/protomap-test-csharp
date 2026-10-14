namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalAttestationMappingChecks
{
    public static void SeasonalAttestationMappingStartsInPlanned()
    {
        var item = SeasonalAttestationMapping.NewSeasonalAttestationMapping("code-251", "name-121");
        Check.Equal(item.Status, SeasonalAttestationMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAttestationMappingMovesToCompleted()
    {
        var item = SeasonalAttestationMapping.NewSeasonalAttestationMapping("code-251", "name-121");
        try
        {
            item.MoveTo(SeasonalAttestationMappingStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAttestationMappingStatus.Completed, "status");
    }

    public static void SeasonalAttestationMappingRejectsMovesFromCancelled()
    {
        var item = SeasonalAttestationMapping.NewSeasonalAttestationMapping("code-251", "name-121");
        item.Status = SeasonalAttestationMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalAttestationMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalAttestationMappingStartsInPlanned();
        SeasonalAttestationMappingMovesToCompleted();
        SeasonalAttestationMappingRejectsMovesFromCancelled();
    }
}
