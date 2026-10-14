namespace Acme.Workforce.Talent.Pool.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool draft.</summary>
public sealed record PoolDraftView(string Code, string Title, string ExpiresAt, string GradeText, string LocaleText, string CeilingText, string AmountText, string SalaryBand, string Active, string SegmentText, string StatusLabel)
{
    public static PoolDraftView ToPoolDraftView(PoolDraft item)
    {
        return new PoolDraftView(
            item.Code,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Locale,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Active}",
            Text.Truncate(item.Segment, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolDraftView> ToPoolDraftViews(List<PoolDraft> items)
    {
        var views = new List<PoolDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPoolDraftView(item));
        }
        return views;
    }

    public static double SumPoolDraftView(PoolDraftView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedCeiling + parsedAmount;
    }
}
