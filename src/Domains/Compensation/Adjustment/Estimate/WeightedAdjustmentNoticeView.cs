namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted adjustment notice.</summary>
public sealed record WeightedAdjustmentNoticeView(string Code, string Title, string Team, string ContactEmailText, string RegionText, string TenureMonths, string StatusLabel)
{
    public static WeightedAdjustmentNoticeView ToWeightedAdjustmentNoticeView(WeightedAdjustmentNotice item)
    {
        return new WeightedAdjustmentNoticeView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Team,
            item.ContactEmail,
            item.Region,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAdjustmentNoticeView> ToWeightedAdjustmentNoticeViews(List<WeightedAdjustmentNotice> items)
    {
        var views = new List<WeightedAdjustmentNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAdjustmentNoticeView(item));
        }
        return views;
    }

    public static double SumWeightedAdjustmentNoticeView(WeightedAdjustmentNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
