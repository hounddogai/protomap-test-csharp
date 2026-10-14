namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped mobility notice.</summary>
public sealed record CappedMobilityNoticeView(string Code, string Name, string CategoryText, string Attempts, string StatusLabel)
{
    public static CappedMobilityNoticeView ToCappedMobilityNoticeView(CappedMobilityNotice item)
    {
        return new CappedMobilityNoticeView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Category,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedMobilityNoticeView> ToCappedMobilityNoticeViews(List<CappedMobilityNotice> items)
    {
        var views = new List<CappedMobilityNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToCappedMobilityNoticeView(item));
        }
        return views;
    }

    public static double SumCappedMobilityNoticeView(CappedMobilityNoticeView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
