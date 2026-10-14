namespace Acme.Workforce.Contractors.Engagement.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement variance.</summary>
public sealed record EngagementVarianceView(string Code, string Label, string LocationText, string Unit, string CostCenter, string NotesText, string Active, string Title, string SalaryBand, string ScoreText, string StatusLabel)
{
    public static EngagementVarianceView ToEngagementVarianceView(EngagementVariance item)
    {
        return new EngagementVarianceView(
            item.Code,
            item.Label,
            item.Location,
            item.Unit,
            item.CostCenter,
            Text.Truncate(item.Notes, 40),
            $"{item.Active}",
            Text.Truncate(item.Title, 40),
            item.SalaryBand,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementVarianceView> ToEngagementVarianceViews(List<EngagementVariance> items)
    {
        var views = new List<EngagementVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementVarianceView(item));
        }
        return views;
    }

    public static double SumEngagementVarianceView(EngagementVarianceView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
