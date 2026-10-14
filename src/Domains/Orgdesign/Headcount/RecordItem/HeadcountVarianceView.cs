namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount variance.</summary>
public sealed record HeadcountVarianceView(string CodeText, string LabelText, string UnitText, string ExpiresAt, string Score, string StatusLabel)
{
    public static HeadcountVarianceView ToHeadcountVarianceView(HeadcountVariance item)
    {
        return new HeadcountVarianceView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Unit,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountVarianceView> ToHeadcountVarianceViews(List<HeadcountVariance> items)
    {
        var views = new List<HeadcountVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountVarianceView(item));
        }
        return views;
    }

    public static double SumHeadcountVarianceView(HeadcountVarianceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedScore;
    }
}
