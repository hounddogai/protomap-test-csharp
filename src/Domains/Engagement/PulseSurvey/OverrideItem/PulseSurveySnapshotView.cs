namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a pulse survey snapshot.</summary>
public sealed record PulseSurveySnapshotView(string Code, string Title, string Retries, string Location, string ExternalRef, string Currency, string StatusLabel)
{
    public static PulseSurveySnapshotView ToPulseSurveySnapshotView(PulseSurveySnapshot item)
    {
        return new PulseSurveySnapshotView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.ExternalRef,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveySnapshotView> ToPulseSurveySnapshotViews(List<PulseSurveySnapshot> items)
    {
        var views = new List<PulseSurveySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveySnapshotView(item));
        }
        return views;
    }

    public static double SumPulseSurveySnapshotView(PulseSurveySnapshotView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
