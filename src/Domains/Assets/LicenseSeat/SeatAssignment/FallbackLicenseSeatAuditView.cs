namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback license seat audit.</summary>
public sealed record FallbackLicenseSeatAuditView(string Code, string Name, string LimitText, string EndsAt, string FloorValueText, string ContactEmailText, string HoursText, string CountryText, string Capacity, string Threshold, string StatusLabel)
{
    public static FallbackLicenseSeatAuditView ToFallbackLicenseSeatAuditView(FallbackLicenseSeatAudit item)
    {
        return new FallbackLicenseSeatAuditView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLicenseSeatAuditView> ToFallbackLicenseSeatAuditViews(List<FallbackLicenseSeatAudit> items)
    {
        var views = new List<FallbackLicenseSeatAuditView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLicenseSeatAuditView(item));
        }
        return views;
    }

    public static double SumFallbackLicenseSeatAuditView(FallbackLicenseSeatAuditView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedEndsAt + parsedFloorValue;
    }
}
