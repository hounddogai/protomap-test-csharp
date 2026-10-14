namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted headcount notice.</summary>
public sealed record WeightedHeadcountNoticeView(string Code, string Label, string Title, string JobCodeText, string Approved, string Currency, string Phone, string QuantityText, string StatusLabel)
{
    public static WeightedHeadcountNoticeView ToWeightedHeadcountNoticeView(WeightedHeadcountNotice item)
    {
        return new WeightedHeadcountNoticeView(
            item.Code,
            item.Label,
            Text.Truncate(item.Title, 40),
            item.JobCode,
            $"{item.Approved}",
            item.Currency,
            item.Phone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedHeadcountNoticeView> ToWeightedHeadcountNoticeViews(List<WeightedHeadcountNotice> items)
    {
        var views = new List<WeightedHeadcountNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedHeadcountNoticeView(item));
        }
        return views;
    }

    public static double SumWeightedHeadcountNoticeView(WeightedHeadcountNoticeView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
