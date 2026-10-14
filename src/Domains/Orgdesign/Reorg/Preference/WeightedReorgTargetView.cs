namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted reorg target.</summary>
public sealed record WeightedReorgTargetView(string Code, string Label, string ExpiresAt, string UpdatedAt, string Approved, string Priority, string StatusLabel)
{
    public static WeightedReorgTargetView ToWeightedReorgTargetView(WeightedReorgTarget item)
    {
        return new WeightedReorgTargetView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedReorgTargetView> ToWeightedReorgTargetViews(List<WeightedReorgTarget> items)
    {
        var views = new List<WeightedReorgTargetView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedReorgTargetView(item));
        }
        return views;
    }

    public static double SumWeightedReorgTargetView(WeightedReorgTargetView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
