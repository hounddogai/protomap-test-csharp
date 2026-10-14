namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a visitor snapshot.</summary>
public sealed record VisitorSnapshotView(string Code, string Label, string VersionText, string ScoreText, string AttemptsText, string ContactEmail, string Quantity, string StatusLabel)
{
    public static VisitorSnapshotView ToVisitorSnapshotView(VisitorSnapshot item)
    {
        return new VisitorSnapshotView(
            item.Code,
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorSnapshotView> ToVisitorSnapshotViews(List<VisitorSnapshot> items)
    {
        var views = new List<VisitorSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorSnapshotView(item));
        }
        return views;
    }

    public static double SumVisitorSnapshotView(VisitorSnapshotView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedScore + (double)parsedAttempts;
    }
}
