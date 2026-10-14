namespace Acme.Workforce.Compensation.PayBand.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted pay band preference.</summary>
public sealed record WeightedPayBandPreferenceView(string CodeText, string Title, string ArchivedText, string Reviewer, string Retries, string Amount, string Name, string Minimum, string Locale, string CostCenter, string StatusLabel)
{
    public static WeightedPayBandPreferenceView ToWeightedPayBandPreferenceView(WeightedPayBandPreference item)
    {
        return new WeightedPayBandPreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            $"{item.Archived}",
            item.Reviewer,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.CostCenter, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPayBandPreferenceView> ToWeightedPayBandPreferenceViews(List<WeightedPayBandPreference> items)
    {
        var views = new List<WeightedPayBandPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPayBandPreferenceView(item));
        }
        return views;
    }

    public static double SumWeightedPayBandPreferenceView(WeightedPayBandPreferenceView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount + parsedMinimum;
    }
}
