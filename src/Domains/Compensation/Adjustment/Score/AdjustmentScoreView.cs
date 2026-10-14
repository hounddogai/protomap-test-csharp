namespace Acme.Workforce.Compensation.Adjustment.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment score.</summary>
public sealed record AdjustmentScoreView(string Code, string Title, string DepartmentText, string Source, string StartsAtText, string Channel, string Currency, string Visible, string ManagerId, string Label, string StatusLabel)
{
    public static AdjustmentScoreView ToAdjustmentScoreView(AdjustmentScore item)
    {
        return new AdjustmentScoreView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Department, 40),
            item.Source,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Currency,
            $"{item.Visible}",
            item.ManagerId,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentScoreView> ToAdjustmentScoreViews(List<AdjustmentScore> items)
    {
        var views = new List<AdjustmentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentScoreView(item));
        }
        return views;
    }

    public static double SumAdjustmentScoreView(AdjustmentScoreView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
