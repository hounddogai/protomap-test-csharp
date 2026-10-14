namespace Acme.Workforce.Payrollops.PayRun.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pay run preference.</summary>
public sealed record RollingPayRunPreferenceView(string Code, string Name, string JobCode, string Description, string Region, string LabelText, string Unit, string VersionText, string ExternalRefText, string Discount, string StatusLabel)
{
    public static RollingPayRunPreferenceView ToRollingPayRunPreferenceView(RollingPayRunPreference item)
    {
        return new RollingPayRunPreferenceView(
            item.Code,
            item.Name,
            Text.Truncate(item.JobCode, 64),
            item.Description,
            item.Region,
            item.Label,
            item.Unit,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPayRunPreferenceView> ToRollingPayRunPreferenceViews(List<RollingPayRunPreference> items)
    {
        var views = new List<RollingPayRunPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPayRunPreferenceView(item));
        }
        return views;
    }

    public static double SumRollingPayRunPreferenceView(RollingPayRunPreferenceView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedDiscount;
    }
}
