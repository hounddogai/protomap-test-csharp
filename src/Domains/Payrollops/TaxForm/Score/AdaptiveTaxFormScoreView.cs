namespace Acme.Workforce.Payrollops.TaxForm.Score;

using System.Globalization;

/// <summary>How screens and exports show a adaptive tax form score.</summary>
public sealed record AdaptiveTaxFormScoreView(string CodeText, string NameText, string Pinned, string EndsAt, string StatusLabel)
{
    public static AdaptiveTaxFormScoreView ToAdaptiveTaxFormScoreView(AdaptiveTaxFormScore item)
    {
        return new AdaptiveTaxFormScoreView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTaxFormScoreView> ToAdaptiveTaxFormScoreViews(List<AdaptiveTaxFormScore> items)
    {
        var views = new List<AdaptiveTaxFormScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTaxFormScoreView(item));
        }
        return views;
    }

    public static double SumAdaptiveTaxFormScoreView(AdaptiveTaxFormScoreView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
