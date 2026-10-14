namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict reorg notice.</summary>
public sealed record StrictReorgNoticeView(string Code, string Label, string CostCenterText, string Checksum, string SourceText, string TenureMonths, string StatusLabel)
{
    public static StrictReorgNoticeView ToStrictReorgNoticeView(StrictReorgNotice item)
    {
        return new StrictReorgNoticeView(
            item.Code,
            item.Label,
            item.CostCenter,
            Text.Truncate(item.Checksum, 40),
            Text.Truncate(item.Source, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReorgNoticeView> ToStrictReorgNoticeViews(List<StrictReorgNotice> items)
    {
        var views = new List<StrictReorgNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReorgNoticeView(item));
        }
        return views;
    }

    public static double SumStrictReorgNoticeView(StrictReorgNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
