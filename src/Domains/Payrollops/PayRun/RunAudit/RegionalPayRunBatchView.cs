namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay run batch.</summary>
public sealed record RegionalPayRunBatchView(string CodeText, string Name, string Skill, string Country, string Version, string StatusLabel)
{
    public static RegionalPayRunBatchView ToRegionalPayRunBatchView(RegionalPayRunBatch item)
    {
        return new RegionalPayRunBatchView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Skill,
            Text.Truncate(item.Country, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayRunBatchView> ToRegionalPayRunBatchViews(List<RegionalPayRunBatch> items)
    {
        var views = new List<RegionalPayRunBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayRunBatchView(item));
        }
        return views;
    }

    public static double SumRegionalPayRunBatchView(RegionalPayRunBatchView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
