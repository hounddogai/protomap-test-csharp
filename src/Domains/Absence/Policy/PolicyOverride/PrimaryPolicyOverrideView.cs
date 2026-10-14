namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary policy override.</summary>
public sealed record PrimaryPolicyOverrideView(string Code, string Name, string Capacity, string Rating, string StartsAt, string BatchSize, string EmployeeId, string Region, string StatusLabel)
{
    public static PrimaryPolicyOverrideView ToPrimaryPolicyOverrideView(PrimaryPolicyOverride item)
    {
        return new PrimaryPolicyOverrideView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPolicyOverrideView> ToPrimaryPolicyOverrideViews(List<PrimaryPolicyOverride> items)
    {
        var views = new List<PrimaryPolicyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPolicyOverrideView(item));
        }
        return views;
    }

    public static double SumPrimaryPolicyOverrideView(PrimaryPolicyOverrideView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRating + (double)parsedStartsAt;
    }
}
