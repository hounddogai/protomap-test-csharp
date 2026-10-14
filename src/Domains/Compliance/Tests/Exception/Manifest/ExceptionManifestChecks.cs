namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionManifestChecks
{
    public static void ExceptionManifestStartsInNew()
    {
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
        Check.Equal(item.Status, ExceptionManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionManifestMovesToDone()
    {
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
        try
        {
            item.MoveTo(ExceptionManifestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionManifestStatus.Done, "status");
    }

    public static void ExceptionManifestRejectsMovesFromFailed()
    {
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
        item.Status = ExceptionManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionManifestStartsInNew();
        ExceptionManifestMovesToDone();
        ExceptionManifestRejectsMovesFromFailed();
    }
}
