namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat preference.</summary>
public sealed record LicenseSeatPreferenceView(string CodeText, string TitleText, string Description, string Checksum, string Hours, string Fte, string Version, string Visible, string StatusLabel)
{
    public static LicenseSeatPreferenceView ToLicenseSeatPreferenceView(LicenseSeatPreference item)
    {
        return new LicenseSeatPreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Description, 80),
            item.Checksum,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatPreferenceView> ToLicenseSeatPreferenceViews(List<LicenseSeatPreference> items)
    {
        var views = new List<LicenseSeatPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatPreferenceView(item));
        }
        return views;
    }

    public static double SumLicenseSeatPreferenceView(LicenseSeatPreferenceView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedHours + parsedFte + (double)parsedVersion;
    }
}
