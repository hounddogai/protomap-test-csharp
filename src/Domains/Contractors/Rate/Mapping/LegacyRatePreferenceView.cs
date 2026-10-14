namespace Acme.Workforce.Contractors.Rate.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a legacy rate preference.</summary>
public sealed record LegacyRatePreferenceView(string Code, string Title, string JobCode, string Active, string Quantity, string Priority, string BatchSize, string GradeText, string Ceiling, string StatusLabel)
{
    public static LegacyRatePreferenceView ToLegacyRatePreferenceView(LegacyRatePreference item)
    {
        return new LegacyRatePreferenceView(
            item.Code,
            item.Title,
            item.JobCode,
            $"{item.Active}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRatePreferenceView> ToLegacyRatePreferenceViews(List<LegacyRatePreference> items)
    {
        var views = new List<LegacyRatePreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRatePreferenceView(item));
        }
        return views;
    }

    public static double SumLegacyRatePreferenceView(LegacyRatePreferenceView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedPriority + (double)parsedBatchSize;
    }
}
