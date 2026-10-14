namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat schedule.</summary>
public sealed record LicenseSeatScheduleView(string Code, string Name, string SalaryBand, string LeaveDays, string OwnerIdText, string DescriptionText, string EmployeeId, string Ratio, string StatusLabel)
{
    public static LicenseSeatScheduleView ToLicenseSeatScheduleView(LicenseSeatSchedule item)
    {
        return new LicenseSeatScheduleView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.SalaryBand,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.Description,
            item.EmployeeId,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatScheduleView> ToLicenseSeatScheduleViews(List<LicenseSeatSchedule> items)
    {
        var views = new List<LicenseSeatScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatScheduleView(item));
        }
        return views;
    }

    public static double SumLicenseSeatScheduleView(LicenseSeatScheduleView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRatio;
    }
}
