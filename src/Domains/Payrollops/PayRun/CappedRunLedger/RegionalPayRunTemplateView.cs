namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay run template.</summary>
public sealed record RegionalPayRunTemplateView(string Code, string Label, string PhoneText, string EndsAt, string TenureMonths, string ChannelText, string Discount, string AmountText, string StatusLabel)
{
    public static RegionalPayRunTemplateView ToRegionalPayRunTemplateView(RegionalPayRunTemplate item)
    {
        return new RegionalPayRunTemplateView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Phone,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayRunTemplateView> ToRegionalPayRunTemplateViews(List<RegionalPayRunTemplate> items)
    {
        var views = new List<RegionalPayRunTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayRunTemplateView(item));
        }
        return views;
    }

    public static double SumRegionalPayRunTemplateView(RegionalPayRunTemplateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedTenureMonths + parsedDiscount;
    }
}
