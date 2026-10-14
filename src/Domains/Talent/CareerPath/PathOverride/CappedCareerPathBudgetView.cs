namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped career path budget.</summary>
public sealed record CappedCareerPathBudgetView(string Code, string Label, string OwnerId, string Grade, string Sequence, string TenureMonths, string EndsAtText, string NameText, string QuantityText, string Discount, string StatusLabel)
{
    public static CappedCareerPathBudgetView ToCappedCareerPathBudgetView(CappedCareerPathBudget item)
    {
        return new CappedCareerPathBudgetView(
            item.Code,
            item.Label,
            item.OwnerId,
            Text.Truncate(item.Grade, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCareerPathBudgetView> ToCappedCareerPathBudgetViews(List<CappedCareerPathBudget> items)
    {
        var views = new List<CappedCareerPathBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCareerPathBudgetView(item));
        }
        return views;
    }

    public static double SumCappedCareerPathBudgetView(CappedCareerPathBudgetView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedTenureMonths + (double)parsedEndsAt;
    }
}
