namespace Acme.Workforce.Absence.Policy.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy profile.</summary>
public sealed record PolicyProfileView(string Code, string LabelText, string CostCenter, string GraceDays, string Category, string StatusLabel)
{
    public static PolicyProfileView ToPolicyProfileView(PolicyProfile item)
    {
        return new PolicyProfileView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyProfileView> ToPolicyProfileViews(List<PolicyProfile> items)
    {
        var views = new List<PolicyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyProfileView(item));
        }
        return views;
    }

    public static double SumPolicyProfileView(PolicyProfileView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
