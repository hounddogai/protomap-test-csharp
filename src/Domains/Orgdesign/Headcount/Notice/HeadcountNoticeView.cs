namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount notice.</summary>
public sealed record HeadcountNoticeView(string Code, string Label, string Summary, string EndsAtText, string RevisionText, string Discount, string RatioText, string LimitText, string JobCode, string CategoryText, string StatusLabel)
{
    public static HeadcountNoticeView ToHeadcountNoticeView(HeadcountNotice item)
    {
        return new HeadcountNoticeView(
            item.Code,
            item.Label,
            Text.Truncate(item.Summary, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountNoticeView> ToHeadcountNoticeViews(List<HeadcountNotice> items)
    {
        var views = new List<HeadcountNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountNoticeView(item));
        }
        return views;
    }

    public static double SumHeadcountNoticeView(HeadcountNoticeView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRevision + parsedDiscount;
    }
}
