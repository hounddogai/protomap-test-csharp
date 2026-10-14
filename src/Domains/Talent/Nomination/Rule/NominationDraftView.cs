namespace Acme.Workforce.Talent.Nomination.Rule;

using System.Globalization;

/// <summary>How screens and exports show a nomination draft.</summary>
public sealed record NominationDraftView(string Code, string Label, string Cost, string Department, string TitleText, string Amount, string Region, string Discount, string ManagerId, string StatusLabel)
{
    public static NominationDraftView ToNominationDraftView(NominationDraft item)
    {
        return new NominationDraftView(
            item.Code,
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationDraftView> ToNominationDraftViews(List<NominationDraft> items)
    {
        var views = new List<NominationDraftView>();
        foreach (var item in items)
        {
            views.Add(ToNominationDraftView(item));
        }
        return views;
    }

    public static double SumNominationDraftView(NominationDraftView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedCost + parsedAmount + parsedDiscount;
    }
}
