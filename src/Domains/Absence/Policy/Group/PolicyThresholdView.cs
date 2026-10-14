namespace Acme.Workforce.Absence.Policy.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy threshold.</summary>
public sealed record PolicyThresholdView(string CodeText, string Name, string ActiveText, string Unit, string MinimumText, string Fte, string StatusLabel)
{
    public static PolicyThresholdView ToPolicyThresholdView(PolicyThreshold item)
    {
        return new PolicyThresholdView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Active}",
            item.Unit,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyThresholdView> ToPolicyThresholdViews(List<PolicyThreshold> items)
    {
        var views = new List<PolicyThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyThresholdView(item));
        }
        return views;
    }

    public static double SumPolicyThresholdView(PolicyThresholdView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFte;
    }
}
