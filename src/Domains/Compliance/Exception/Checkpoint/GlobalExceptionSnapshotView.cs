namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a global exception snapshot.</summary>
public sealed record GlobalExceptionSnapshotView(string Code, string TitleText, string RatingText, string JobCode, string NameText, string StatusLabel)
{
    public static GlobalExceptionSnapshotView ToGlobalExceptionSnapshotView(GlobalExceptionSnapshot item)
    {
        return new GlobalExceptionSnapshotView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalExceptionSnapshotView> ToGlobalExceptionSnapshotViews(List<GlobalExceptionSnapshot> items)
    {
        var views = new List<GlobalExceptionSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalExceptionSnapshotView(item));
        }
        return views;
    }

    public static double SumGlobalExceptionSnapshotView(GlobalExceptionSnapshotView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
