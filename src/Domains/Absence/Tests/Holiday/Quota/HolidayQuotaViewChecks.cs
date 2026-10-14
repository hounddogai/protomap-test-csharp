namespace Acme.Workforce.Absence.Tests.Holiday.Quota;

using Acme.Workforce.Absence.Holiday.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class HolidayQuotaViewChecks
{
    public static void HolidayQuotaViewShowsTheStatusInCapitals()
    {
        var item = HolidayQuota.NewHolidayQuota("code-672", "name-66");
        var view = HolidayQuotaView.ToHolidayQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayQuotaView.ToHolidayQuotaViews(new List<HolidayQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayQuotaViewShowsTheStatusInCapitals();
    }
}
