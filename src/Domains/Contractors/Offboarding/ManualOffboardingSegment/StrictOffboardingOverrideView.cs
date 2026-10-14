namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict offboarding override.</summary>
public sealed record StrictOffboardingOverrideView(string Code, string Title, string Grade, string Quantity, string Visible, string Ceiling, string StatusLabel)
{
    public static StrictOffboardingOverrideView ToStrictOffboardingOverrideView(StrictOffboardingOverride item)
    {
        return new StrictOffboardingOverrideView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Grade,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictOffboardingOverrideView> ToStrictOffboardingOverrideViews(List<StrictOffboardingOverride> items)
    {
        var views = new List<StrictOffboardingOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToStrictOffboardingOverrideView(item));
        }
        return views;
    }

    public static double SumStrictOffboardingOverrideView(StrictOffboardingOverrideView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedCeiling;
    }
}
