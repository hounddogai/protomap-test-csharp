namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional succession plan adjustment.</summary>
public sealed record RegionalSuccessionPlanAdjustmentView(string Code, string NameText, string ChannelText, string BatchSizeText, string ContactEmail, string Retries, string Amount, string StatusLabel)
{
    public static RegionalSuccessionPlanAdjustmentView ToRegionalSuccessionPlanAdjustmentView(RegionalSuccessionPlanAdjustment item)
    {
        return new RegionalSuccessionPlanAdjustmentView(
            item.Code,
            item.Name,
            Text.Truncate(item.Channel, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSuccessionPlanAdjustmentView> ToRegionalSuccessionPlanAdjustmentViews(List<RegionalSuccessionPlanAdjustment> items)
    {
        var views = new List<RegionalSuccessionPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSuccessionPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumRegionalSuccessionPlanAdjustmentView(RegionalSuccessionPlanAdjustmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedRetries + parsedAmount;
    }
}
