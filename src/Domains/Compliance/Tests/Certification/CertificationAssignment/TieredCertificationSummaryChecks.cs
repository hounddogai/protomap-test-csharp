namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class TieredCertificationSummaryChecks
{
    public static void TieredCertificationSummaryStartsInRequested()
    {
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
        Check.Equal(item.Status, TieredCertificationSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCertificationSummaryMovesToGranted()
    {
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
        try
        {
            item.MoveTo(TieredCertificationSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCertificationSummaryStatus.Granted, "status");
    }

    public static void TieredCertificationSummaryRejectsMovesFromExpired()
    {
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
        item.Status = TieredCertificationSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredCertificationSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredCertificationSummaryStartsInRequested();
        TieredCertificationSummaryMovesToGranted();
        TieredCertificationSummaryRejectsMovesFromExpired();
    }
}
