namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using System.Globalization;

/// <summary>How screens and exports show a position draft.</summary>
public sealed record PositionDraftView(string Code, string NameText, string Summary, string PinnedText, string Location, string Score, string StatusLabel)
{
    public static PositionDraftView ToPositionDraftView(PositionDraft item)
    {
        return new PositionDraftView(
            item.Code,
            item.Name,
            item.Summary,
            $"{item.Pinned}",
            item.Location,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionDraftView> ToPositionDraftViews(List<PositionDraft> items)
    {
        var views = new List<PositionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPositionDraftView(item));
        }
        return views;
    }

    public static double SumPositionDraftView(PositionDraftView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
