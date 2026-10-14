namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ManualRateProfileChecks
{
    public static void ManualRateProfileStartsInRequested()
    {
        var item = ManualRateProfile.NewManualRateProfile("code-24", "label-472");
        Check.Equal(item.Status, ManualRateProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRateProfileMovesToRevoked()
    {
        var item = ManualRateProfile.NewManualRateProfile("code-24", "label-472");
        try
        {
            item.MoveTo(ManualRateProfileStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRateProfileStatus.Revoked, "status");
    }

    public static void ManualRateProfileRejectsMovesFromExpired()
    {
        var item = ManualRateProfile.NewManualRateProfile("code-24", "label-472");
        item.Status = ManualRateProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualRateProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualRateProfileStartsInRequested();
        ManualRateProfileMovesToRevoked();
        ManualRateProfileRejectsMovesFromExpired();
    }
}
