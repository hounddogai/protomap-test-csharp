namespace Acme.Workforce.Assets.Assignment.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a assignment draft.</summary>
public sealed record AssignmentDraftView(string Code, string Label, string Retries, string Rating, string ExpiresAt, string Weight, string Fte, string FloorValue, string WindowDays, string StatusLabel)
{
    public static AssignmentDraftView ToAssignmentDraftView(AssignmentDraft item)
    {
        return new AssignmentDraftView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentDraftView> ToAssignmentDraftViews(List<AssignmentDraft> items)
    {
        var views = new List<AssignmentDraftView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentDraftView(item));
        }
        return views;
    }

    public static double SumAssignmentDraftView(AssignmentDraftView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRating + (double)parsedExpiresAt;
    }
}
