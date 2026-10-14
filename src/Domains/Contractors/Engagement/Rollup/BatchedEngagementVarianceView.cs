namespace Acme.Workforce.Contractors.Engagement.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched engagement variance.</summary>
public sealed record BatchedEngagementVarianceView(string Code, string Label, string AmountText, string Skill, string CountryText, string StatusLabel)
{
    public static BatchedEngagementVarianceView ToBatchedEngagementVarianceView(BatchedEngagementVariance item)
    {
        return new BatchedEngagementVarianceView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedEngagementVarianceView> ToBatchedEngagementVarianceViews(List<BatchedEngagementVariance> items)
    {
        var views = new List<BatchedEngagementVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedEngagementVarianceView(item));
        }
        return views;
    }

    public static double SumBatchedEngagementVarianceView(BatchedEngagementVarianceView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
