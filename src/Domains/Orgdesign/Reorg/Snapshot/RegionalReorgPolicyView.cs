namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional reorg policy.</summary>
public sealed record RegionalReorgPolicyView(string CodeText, string Name, string Retries, string GradeText, string CostCenter, string EmployeeId, string Checksum, string StatusLabel)
{
    public static RegionalReorgPolicyView ToRegionalReorgPolicyView(RegionalReorgPolicy item)
    {
        return new RegionalReorgPolicyView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.CostCenter,
            item.EmployeeId,
            Text.Truncate(item.Checksum, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalReorgPolicyView> ToRegionalReorgPolicyViews(List<RegionalReorgPolicy> items)
    {
        var views = new List<RegionalReorgPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalReorgPolicyView(item));
        }
        return views;
    }

    public static double SumRegionalReorgPolicyView(RegionalReorgPolicyView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
