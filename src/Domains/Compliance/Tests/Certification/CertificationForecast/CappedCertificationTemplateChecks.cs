namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class CappedCertificationTemplateChecks
{
    public static void CappedCertificationTemplateStartsInPlanned()
    {
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("code-263", "label-690");
        Check.Equal(item.Status, CappedCertificationTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCertificationTemplateMovesToInProgress()
    {
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("code-263", "label-690");
        try
        {
            item.MoveTo(CappedCertificationTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCertificationTemplateStatus.InProgress, "status");
    }

    public static void CappedCertificationTemplateRejectsMovesFromCancelled()
    {
        var item = CappedCertificationTemplate.NewCappedCertificationTemplate("code-263", "label-690");
        item.Status = CappedCertificationTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedCertificationTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedCertificationTemplateStartsInPlanned();
        CappedCertificationTemplateMovesToInProgress();
        CappedCertificationTemplateRejectsMovesFromCancelled();
    }
}
