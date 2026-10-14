namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalAttestationPreferenceChecks
{
    public static void RegionalAttestationPreferenceStartsInPlanned()
    {
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
        Check.Equal(item.Status, RegionalAttestationPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAttestationPreferenceMovesToCancelled()
    {
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
        try
        {
            item.MoveTo(RegionalAttestationPreferenceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAttestationPreferenceStatus.Cancelled, "status");
    }

    public static void RegionalAttestationPreferenceRejectsMovesFromCancelled()
    {
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
        item.Status = RegionalAttestationPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalAttestationPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalAttestationPreferenceStartsInPlanned();
        RegionalAttestationPreferenceMovesToCancelled();
        RegionalAttestationPreferenceRejectsMovesFromCancelled();
    }
}
