namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty schedule.</summary>
public sealed record WarrantyScheduleView(string Code, string LabelText, string CostCenter, string Department, string CeilingText, string ChecksumText, string Hours, string LocationText, string StatusLabel)
{
    public static WarrantyScheduleView ToWarrantyScheduleView(WarrantySchedule item)
    {
        return new WarrantyScheduleView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 80),
            item.Department,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyScheduleView> ToWarrantyScheduleViews(List<WarrantySchedule> items)
    {
        var views = new List<WarrantyScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyScheduleView(item));
        }
        return views;
    }

    public static double SumWarrantyScheduleView(WarrantyScheduleView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedHours;
    }
}
