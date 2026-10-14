namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalPositionVarianceChecks
{
    public static void GlobalPositionVarianceStartsInRequested()
    {
        var item = GlobalPositionVariance.NewGlobalPositionVariance("code-155", "title-406");
        Check.Equal(item.Status, GlobalPositionVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPositionVarianceMovesToGranted()
    {
        var item = GlobalPositionVariance.NewGlobalPositionVariance("code-155", "title-406");
        try
        {
            item.MoveTo(GlobalPositionVarianceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPositionVarianceStatus.Granted, "status");
    }

    public static void GlobalPositionVarianceRejectsMovesFromExpired()
    {
        var item = GlobalPositionVariance.NewGlobalPositionVariance("code-155", "title-406");
        item.Status = GlobalPositionVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalPositionVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalPositionVarianceStartsInRequested();
        GlobalPositionVarianceMovesToGranted();
        GlobalPositionVarianceRejectsMovesFromExpired();
    }
}
