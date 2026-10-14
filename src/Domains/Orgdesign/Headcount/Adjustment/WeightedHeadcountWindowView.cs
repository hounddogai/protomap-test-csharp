namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted headcount window.</summary>
public sealed record WeightedHeadcountWindowView(string Code, string LabelText, string ManagerId, string Weight, string ChannelText, string LocationText, string CurrencyText, string MinimumText, string EmployeeId, string Attempts, string StatusLabel)
{
    public static WeightedHeadcountWindowView ToWeightedHeadcountWindowView(WeightedHeadcountWindow item)
    {
        return new WeightedHeadcountWindowView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.ManagerId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Location,
            item.Currency,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedHeadcountWindowView> ToWeightedHeadcountWindowViews(List<WeightedHeadcountWindow> items)
    {
        var views = new List<WeightedHeadcountWindowView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedHeadcountWindowView(item));
        }
        return views;
    }

    public static double SumWeightedHeadcountWindowView(WeightedHeadcountWindowView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedWeight + parsedMinimum + (double)parsedAttempts;
    }
}
