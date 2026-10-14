namespace Acme.Workforce.Talent.Pool.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool agreement.</summary>
public sealed record PoolAgreementView(string Code, string Title, string LabelText, string Segment, string Region, string Reviewer, string Score, string StatusLabel)
{
    public static PoolAgreementView ToPoolAgreementView(PoolAgreement item)
    {
        return new PoolAgreementView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Label,
            item.Segment,
            Text.Truncate(item.Region, 120),
            item.Reviewer,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolAgreementView> ToPoolAgreementViews(List<PoolAgreement> items)
    {
        var views = new List<PoolAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPoolAgreementView(item));
        }
        return views;
    }

    public static double SumPoolAgreementView(PoolAgreementView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
