namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanManifestChecks
{
    public static void SuccessionPlanManifestStartsInRequested()
    {
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("code-899", "label-794");
        Check.Equal(item.Status, SuccessionPlanManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanManifestMovesToGranted()
    {
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("code-899", "label-794");
        try
        {
            item.MoveTo(SuccessionPlanManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanManifestStatus.Granted, "status");
    }

    public static void SuccessionPlanManifestRejectsMovesFromExpired()
    {
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("code-899", "label-794");
        item.Status = SuccessionPlanManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(SuccessionPlanManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanManifestStartsInRequested();
        SuccessionPlanManifestMovesToGranted();
        SuccessionPlanManifestRejectsMovesFromExpired();
    }
}
