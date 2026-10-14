namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg notice.</summary>
public sealed record ReorgNoticeView(string CodeText, string Title, string WeightText, string TeamText, string Checksum, string StatusLabel)
{
    public static ReorgNoticeView ToReorgNoticeView(ReorgNotice item)
    {
        return new ReorgNoticeView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgNoticeView> ToReorgNoticeViews(List<ReorgNotice> items)
    {
        var views = new List<ReorgNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToReorgNoticeView(item));
        }
        return views;
    }

    public static double SumReorgNoticeView(ReorgNoticeView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
