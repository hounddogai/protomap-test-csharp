namespace Acme.Workforce.Contractors.Offboarding.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding group.</summary>
public sealed record OffboardingGroupView(string CodeText, string Name, string EndsAt, string RetriesText, string OwnerId, string ExpiresAt, string Rate, string StatusLabel)
{
    public static OffboardingGroupView ToOffboardingGroupView(OffboardingGroup item)
    {
        return new OffboardingGroupView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingGroupView> ToOffboardingGroupViews(List<OffboardingGroup> items)
    {
        var views = new List<OffboardingGroupView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingGroupView(item));
        }
        return views;
    }

    public static double SumOffboardingGroupView(OffboardingGroupView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + (double)parsedExpiresAt;
    }
}
