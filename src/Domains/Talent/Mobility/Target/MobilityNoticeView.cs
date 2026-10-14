namespace Acme.Workforce.Talent.Mobility.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility notice.</summary>
public sealed record MobilityNoticeView(string CodeText, string Name, string Limit, string Priority, string Unit, string TenureMonthsText, string CostText, string WindowDays, string Amount, string RateText, string StatusLabel)
{
    public static MobilityNoticeView ToMobilityNoticeView(MobilityNotice item)
    {
        return new MobilityNoticeView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityNoticeView> ToMobilityNoticeViews(List<MobilityNotice> items)
    {
        var views = new List<MobilityNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityNoticeView(item));
        }
        return views;
    }

    public static double SumMobilityNoticeView(MobilityNoticeView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedPriority + (double)parsedTenureMonths;
    }
}
