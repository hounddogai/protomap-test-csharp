namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationSlotViewChecks
{
    public static void CertificationSlotViewShowsTheStatusInCapitals()
    {
        var item = CertificationSlot.NewCertificationSlot("code-958", "title-326");
        var view = CertificationSlotView.ToCertificationSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationSlotView.ToCertificationSlotViews(new List<CertificationSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationSlotViewShowsTheStatusInCapitals();
    }
}
