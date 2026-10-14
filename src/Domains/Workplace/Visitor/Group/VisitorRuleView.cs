namespace Acme.Workforce.Workplace.Visitor.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor rule.</summary>
public sealed record VisitorRuleView(string CodeText, string Title, string Approved, string ScoreText, string BatchSize, string Maximum, string Threshold, string FloorValueText, string Team, string StatusLabel)
{
    public static VisitorRuleView ToVisitorRuleView(VisitorRule item)
    {
        return new VisitorRuleView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Approved}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorRuleView> ToVisitorRuleViews(List<VisitorRule> items)
    {
        var views = new List<VisitorRuleView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorRuleView(item));
        }
        return views;
    }

    public static double SumVisitorRuleView(VisitorRuleView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedBatchSize + parsedMaximum;
    }
}
