namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class FallbackCertificationBundleChecks
{
    public static void FallbackCertificationBundleStartsInNew()
    {
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("code-999", "name-560");
        Check.Equal(item.Status, FallbackCertificationBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCertificationBundleMovesToRunning()
    {
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("code-999", "name-560");
        try
        {
            item.MoveTo(FallbackCertificationBundleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCertificationBundleStatus.Running, "status");
    }

    public static void FallbackCertificationBundleRejectsMovesFromFailed()
    {
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("code-999", "name-560");
        item.Status = FallbackCertificationBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackCertificationBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackCertificationBundleStartsInNew();
        FallbackCertificationBundleMovesToRunning();
        FallbackCertificationBundleRejectsMovesFromFailed();
    }
}
