namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat override.</summary>
public sealed record LicenseSeatOverrideView(string CodeText, string Label, string Category, string ActiveText, string Team, string Threshold, string ChecksumText, string FloorValue, string UnitText, string VersionText, string StatusLabel)
{
    public static LicenseSeatOverrideView ToLicenseSeatOverrideView(LicenseSeatOverride item)
    {
        return new LicenseSeatOverrideView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Category, 120),
            $"{item.Active}",
            item.Team,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatOverrideView> ToLicenseSeatOverrideViews(List<LicenseSeatOverride> items)
    {
        var views = new List<LicenseSeatOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatOverrideView(item));
        }
        return views;
    }

    public static double SumLicenseSeatOverrideView(LicenseSeatOverrideView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedFloorValue + (double)parsedVersion;
    }
}
