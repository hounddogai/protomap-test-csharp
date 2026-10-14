namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using System.Globalization;

/// <summary>How screens and exports show a primary headcount draft.</summary>
public sealed record PrimaryHeadcountDraftView(string Code, string Title, string RateText, string Approved, string StatusLabel)
{
    public static PrimaryHeadcountDraftView ToPrimaryHeadcountDraftView(PrimaryHeadcountDraft item)
    {
        return new PrimaryHeadcountDraftView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryHeadcountDraftView> ToPrimaryHeadcountDraftViews(List<PrimaryHeadcountDraft> items)
    {
        var views = new List<PrimaryHeadcountDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryHeadcountDraftView(item));
        }
        return views;
    }

    public static double SumPrimaryHeadcountDraftView(PrimaryHeadcountDraftView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
