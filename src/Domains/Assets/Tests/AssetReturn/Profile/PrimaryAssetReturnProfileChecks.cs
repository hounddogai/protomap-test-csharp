namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryAssetReturnProfileChecks
{
    public static void PrimaryAssetReturnProfileStartsInRequested()
    {
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
        Check.Equal(item.Status, PrimaryAssetReturnProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAssetReturnProfileMovesToGranted()
    {
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
        try
        {
            item.MoveTo(PrimaryAssetReturnProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAssetReturnProfileStatus.Granted, "status");
    }

    public static void PrimaryAssetReturnProfileRejectsMovesFromExpired()
    {
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
        item.Status = PrimaryAssetReturnProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryAssetReturnProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryAssetReturnProfileStartsInRequested();
        PrimaryAssetReturnProfileMovesToGranted();
        PrimaryAssetReturnProfileRejectsMovesFromExpired();
    }
}
