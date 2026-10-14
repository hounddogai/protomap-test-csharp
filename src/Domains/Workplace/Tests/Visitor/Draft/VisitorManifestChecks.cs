namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorManifestChecks
{
    public static void VisitorManifestStartsInNew()
    {
        var item = VisitorManifest.NewVisitorManifest("code-392", "name-460");
        Check.Equal(item.Status, VisitorManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorManifestMovesToQueued()
    {
        var item = VisitorManifest.NewVisitorManifest("code-392", "name-460");
        try
        {
            item.MoveTo(VisitorManifestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorManifestStatus.Queued, "status");
    }

    public static void VisitorManifestRejectsMovesFromFailed()
    {
        var item = VisitorManifest.NewVisitorManifest("code-392", "name-460");
        item.Status = VisitorManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorManifestStartsInNew();
        VisitorManifestMovesToQueued();
        VisitorManifestRejectsMovesFromFailed();
    }
}
