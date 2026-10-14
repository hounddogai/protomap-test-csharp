namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling retro pay summary.</summary>
public sealed record RollingRetroPaySummaryView(string Code, string Name, string CurrencyText, string Rate, string Ratio, string ChannelText, string StatusLabel)
{
    public static RollingRetroPaySummaryView ToRollingRetroPaySummaryView(RollingRetroPaySummary item)
    {
        return new RollingRetroPaySummaryView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Currency, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRetroPaySummaryView> ToRollingRetroPaySummaryViews(List<RollingRetroPaySummary> items)
    {
        var views = new List<RollingRetroPaySummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRetroPaySummaryView(item));
        }
        return views;
    }

    public static double SumRollingRetroPaySummaryView(RollingRetroPaySummaryView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + parsedRatio;
    }
}
