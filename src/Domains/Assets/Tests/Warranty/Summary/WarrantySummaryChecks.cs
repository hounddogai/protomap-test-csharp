namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantySummaryChecks
{
    public static void WarrantySummaryStartsInIdle()
    {
        var item = WarrantySummary.NewWarrantySummary("code-295", "label-67");
        Check.Equal(item.Status, WarrantySummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantySummaryMovesToWarming()
    {
        var item = WarrantySummary.NewWarrantySummary("code-295", "label-67");
        try
        {
            item.MoveTo(WarrantySummaryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantySummaryStatus.Warming, "status");
    }

    public static void WarrantySummaryRejectsMovesFromStopped()
    {
        var item = WarrantySummary.NewWarrantySummary("code-295", "label-67");
        item.Status = WarrantySummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantySummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantySummaryStartsInIdle();
        WarrantySummaryMovesToWarming();
        WarrantySummaryRejectsMovesFromStopped();
    }
}
