namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict policy entry.</summary>
public sealed record StrictPolicyEntryView(string CodeText, string Name, string OwnerId, string MinimumText, string Discount, string RetriesText, string LocationText, string UpdatedAt, string Quantity, string StatusLabel)
{
    public static StrictPolicyEntryView ToStrictPolicyEntryView(StrictPolicyEntry item)
    {
        return new StrictPolicyEntryView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 64),
            item.OwnerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyEntryView> ToStrictPolicyEntryViews(List<StrictPolicyEntry> items)
    {
        var views = new List<StrictPolicyEntryView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyEntryView(item));
        }
        return views;
    }

    public static double SumStrictPolicyEntryView(StrictPolicyEntryView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedDiscount + (double)parsedRetries;
    }
}
