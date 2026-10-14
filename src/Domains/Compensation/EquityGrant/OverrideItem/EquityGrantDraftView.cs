namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant draft.</summary>
public sealed record EquityGrantDraftView(string Code, string Title, string Rate, string CurrencyText, string Notes, string WeightText, string JobCode, string UpdatedAt, string CreatedAtText, string StatusLabel)
{
    public static EquityGrantDraftView ToEquityGrantDraftView(EquityGrantDraft item)
    {
        return new EquityGrantDraftView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.Notes,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantDraftView> ToEquityGrantDraftViews(List<EquityGrantDraft> items)
    {
        var views = new List<EquityGrantDraftView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantDraftView(item));
        }
        return views;
    }

    public static double SumEquityGrantDraftView(EquityGrantDraftView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRate + parsedWeight + (double)parsedUpdatedAt;
    }
}
