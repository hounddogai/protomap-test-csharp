namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback license seat record.</summary>
public sealed record FallbackLicenseSeatRecordView(string Code, string Name, string Timezone, string CreatedAtText, string Version, string Source, string StatusLabel)
{
    public static FallbackLicenseSeatRecordView ToFallbackLicenseSeatRecordView(FallbackLicenseSeatRecord item)
    {
        return new FallbackLicenseSeatRecordView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Timezone,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLicenseSeatRecordView> ToFallbackLicenseSeatRecordViews(List<FallbackLicenseSeatRecord> items)
    {
        var views = new List<FallbackLicenseSeatRecordView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLicenseSeatRecordView(item));
        }
        return views;
    }

    public static double SumFallbackLicenseSeatRecordView(FallbackLicenseSeatRecordView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedVersion;
    }
}
