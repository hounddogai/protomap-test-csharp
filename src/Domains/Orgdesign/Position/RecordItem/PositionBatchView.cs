namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position batch.</summary>
public sealed record PositionBatchView(string CodeText, string Title, string ScoreText, string Name, string Retries, string StatusLabel)
{
    public static PositionBatchView ToPositionBatchView(PositionBatch item)
    {
        return new PositionBatchView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionBatchView> ToPositionBatchViews(List<PositionBatch> items)
    {
        var views = new List<PositionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPositionBatchView(item));
        }
        return views;
    }

    public static double SumPositionBatchView(PositionBatchView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedRetries;
    }
}
