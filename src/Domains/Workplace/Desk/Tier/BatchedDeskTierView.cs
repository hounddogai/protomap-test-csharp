namespace Acme.Workforce.Workplace.Desk.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched desk tier.</summary>
public sealed record BatchedDeskTierView(string Code, string Label, string CountryText, string RegionText, string NameText, string GradeText, string AmountText, string StatusLabel)
{
    public static BatchedDeskTierView ToBatchedDeskTierView(BatchedDeskTier item)
    {
        return new BatchedDeskTierView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Country,
            item.Region,
            Text.Truncate(item.Name, 64),
            item.Grade,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedDeskTierView> ToBatchedDeskTierViews(List<BatchedDeskTier> items)
    {
        var views = new List<BatchedDeskTierView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedDeskTierView(item));
        }
        return views;
    }

    public static double SumBatchedDeskTierView(BatchedDeskTierView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
