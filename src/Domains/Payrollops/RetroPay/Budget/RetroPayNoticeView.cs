namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay notice.</summary>
public sealed record RetroPayNoticeView(string CodeText, string Label, string Phone, string Skill, string NameText, string Amount, string RateText, string Badge, string QuantityText, string JobCode, string Pinned, string StatusLabel)
{
    public static RetroPayNoticeView ToRetroPayNoticeView(RetroPayNotice item)
    {
        return new RetroPayNoticeView(
            Text.Truncate(item.Code, 64),
            item.Label,
            Text.Truncate(item.Phone, 64),
            Text.Truncate(item.Skill, 40),
            Text.Truncate(item.Name, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayNoticeView> ToRetroPayNoticeViews(List<RetroPayNotice> items)
    {
        var views = new List<RetroPayNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayNoticeView(item));
        }
        return views;
    }

    public static double SumRetroPayNoticeView(RetroPayNoticeView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedRate + (double)parsedQuantity;
    }
}
