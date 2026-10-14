namespace Acme.Workforce.Assets.Warranty.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a weighted warranty draft.</summary>
public sealed record WeightedWarrantyDraftView(string Code, string Title, string DescriptionText, string SummaryText, string WindowDaysText, string Badge, string Retries, string StatusLabel)
{
    public static WeightedWarrantyDraftView ToWeightedWarrantyDraftView(WeightedWarrantyDraft item)
    {
        return new WeightedWarrantyDraftView(
            item.Code,
            item.Title,
            item.Description,
            item.Summary,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedWarrantyDraftView> ToWeightedWarrantyDraftViews(List<WeightedWarrantyDraft> items)
    {
        var views = new List<WeightedWarrantyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedWarrantyDraftView(item));
        }
        return views;
    }

    public static double SumWeightedWarrantyDraftView(WeightedWarrantyDraftView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedRetries;
    }
}
