namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted retro pay notice.</summary>
public sealed record WeightedRetroPayNoticeView(string Code, string LabelText, string Quantity, string Unit, string DescriptionText, string Phone, string StatusLabel)
{
    public static WeightedRetroPayNoticeView ToWeightedRetroPayNoticeView(WeightedRetroPayNotice item)
    {
        return new WeightedRetroPayNoticeView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Description,
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedRetroPayNoticeView> ToWeightedRetroPayNoticeViews(List<WeightedRetroPayNotice> items)
    {
        var views = new List<WeightedRetroPayNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedRetroPayNoticeView(item));
        }
        return views;
    }

    public static double SumWeightedRetroPayNoticeView(WeightedRetroPayNoticeView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
