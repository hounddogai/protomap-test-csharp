namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal cost model milestone.</summary>
public sealed record SeasonalCostModelMilestoneView(string Code, string LabelText, string Location, string SummaryText, string GraceDays, string Amount, string CreatedAtText, string StatusLabel)
{
    public static SeasonalCostModelMilestoneView ToSeasonalCostModelMilestoneView(SeasonalCostModelMilestone item)
    {
        return new SeasonalCostModelMilestoneView(
            item.Code,
            item.Label,
            item.Location,
            Text.Truncate(item.Summary, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalCostModelMilestoneView> ToSeasonalCostModelMilestoneViews(List<SeasonalCostModelMilestone> items)
    {
        var views = new List<SeasonalCostModelMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalCostModelMilestoneView(item));
        }
        return views;
    }

    public static double SumSeasonalCostModelMilestoneView(SeasonalCostModelMilestoneView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedAmount + (double)parsedCreatedAt;
    }
}
