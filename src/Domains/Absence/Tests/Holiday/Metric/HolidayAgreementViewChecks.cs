namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class HolidayAgreementViewChecks
{
    public static void HolidayAgreementViewShowsTheStatusInCapitals()
    {
        var item = HolidayAgreement.NewHolidayAgreement("code-290", "name-447");
        var view = HolidayAgreementView.ToHolidayAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayAgreementView.ToHolidayAgreementViews(new List<HolidayAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayAgreementViewShowsTheStatusInCapitals();
    }
}
