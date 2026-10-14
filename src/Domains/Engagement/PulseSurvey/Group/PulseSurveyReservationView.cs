namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey reservation.</summary>
public sealed record PulseSurveyReservationView(string CodeText, string Title, string CeilingText, string RevisionText, string Label, string BatchSizeText, string Fte, string Quantity, string FloorValue, string StatusLabel)
{
    public static PulseSurveyReservationView ToPulseSurveyReservationView(PulseSurveyReservation item)
    {
        return new PulseSurveyReservationView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyReservationView> ToPulseSurveyReservationViews(List<PulseSurveyReservation> items)
    {
        var views = new List<PulseSurveyReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyReservationView(item));
        }
        return views;
    }

    public static double SumPulseSurveyReservationView(PulseSurveyReservationView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRevision + (double)parsedBatchSize;
    }
}
