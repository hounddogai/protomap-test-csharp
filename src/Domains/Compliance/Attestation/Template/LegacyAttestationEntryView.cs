namespace Acme.Workforce.Compliance.Attestation.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy attestation entry.</summary>
public sealed record LegacyAttestationEntryView(string Code, string Label, string Hours, string Timezone, string Ratio, string JobCode, string Region, string StatusLabel)
{
    public static LegacyAttestationEntryView ToLegacyAttestationEntryView(LegacyAttestationEntry item)
    {
        return new LegacyAttestationEntryView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAttestationEntryView> ToLegacyAttestationEntryViews(List<LegacyAttestationEntry> items)
    {
        var views = new List<LegacyAttestationEntryView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAttestationEntryView(item));
        }
        return views;
    }

    public static double SumLegacyAttestationEntryView(LegacyAttestationEntryView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedHours + parsedRatio;
    }
}
