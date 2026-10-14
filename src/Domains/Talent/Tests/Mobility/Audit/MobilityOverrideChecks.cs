namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityOverrideChecks
{
    public static void MobilityOverrideStartsInCreated()
    {
        var item = MobilityOverride.NewMobilityOverride("code-335", "name-773");
        Check.Equal(item.Status, MobilityOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityOverrideMovesToValidated()
    {
        var item = MobilityOverride.NewMobilityOverride("code-335", "name-773");
        try
        {
            item.MoveTo(MobilityOverrideStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityOverrideStatus.Validated, "status");
    }

    public static void MobilityOverrideRejectsMovesFromRetired()
    {
        var item = MobilityOverride.NewMobilityOverride("code-335", "name-773");
        item.Status = MobilityOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(MobilityOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        MobilityOverrideStartsInCreated();
        MobilityOverrideMovesToValidated();
        MobilityOverrideRejectsMovesFromRetired();
    }
}
