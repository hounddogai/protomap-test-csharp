namespace Acme.Workforce.Contractors.Offboarding.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding target.</summary>
public sealed record OffboardingTargetView(string CodeText, string Title, string UpdatedAt, string Archived, string PinnedText, string Name, string StatusLabel)
{
    public static OffboardingTargetView ToOffboardingTargetView(OffboardingTarget item)
    {
        return new OffboardingTargetView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Pinned}",
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingTargetView> ToOffboardingTargetViews(List<OffboardingTarget> items)
    {
        var views = new List<OffboardingTargetView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingTargetView(item));
        }
        return views;
    }

    public static double SumOffboardingTargetView(OffboardingTargetView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
