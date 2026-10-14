namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay rollup.</summary>
public sealed record RetroPayRollupView(string CodeText, string Title, string CostCenterText, string Source, string ExpiresAt, string LabelText, string Locale, string Country, string OwnerId, string Badge, string StatusLabel)
{
    public static RetroPayRollupView ToRetroPayRollupView(RetroPayRollup item)
    {
        return new RetroPayRollupView(
            item.Code,
            item.Title,
            Text.Truncate(item.CostCenter, 64),
            item.Source,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            item.Locale,
            item.Country,
            Text.Truncate(item.OwnerId, 80),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayRollupView> ToRetroPayRollupViews(List<RetroPayRollup> items)
    {
        var views = new List<RetroPayRollupView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayRollupView(item));
        }
        return views;
    }

    public static double SumRetroPayRollupView(RetroPayRollupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
