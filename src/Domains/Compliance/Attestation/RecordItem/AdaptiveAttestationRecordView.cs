namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a adaptive attestation record.</summary>
public sealed record AdaptiveAttestationRecordView(string Code, string Label, string TimezoneText, string Archived, string Rating, string StatusLabel)
{
    public static AdaptiveAttestationRecordView ToAdaptiveAttestationRecordView(AdaptiveAttestationRecord item)
    {
        return new AdaptiveAttestationRecordView(
            item.Code,
            item.Label,
            item.Timezone,
            $"{item.Archived}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAttestationRecordView> ToAdaptiveAttestationRecordViews(List<AdaptiveAttestationRecord> items)
    {
        var views = new List<AdaptiveAttestationRecordView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAttestationRecordView(item));
        }
        return views;
    }

    public static double SumAdaptiveAttestationRecordView(AdaptiveAttestationRecordView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
