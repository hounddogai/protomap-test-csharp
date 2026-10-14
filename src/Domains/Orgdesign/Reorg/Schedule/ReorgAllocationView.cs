namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg allocation.</summary>
public sealed record ReorgAllocationView(string CodeText, string Name, string ThresholdText, string ManagerId, string Skill, string Version, string StatusLabel)
{
    public static ReorgAllocationView ToReorgAllocationView(ReorgAllocation item)
    {
        return new ReorgAllocationView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Skill,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgAllocationView> ToReorgAllocationViews(List<ReorgAllocation> items)
    {
        var views = new List<ReorgAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToReorgAllocationView(item));
        }
        return views;
    }

    public static double SumReorgAllocationView(ReorgAllocationView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedVersion;
    }
}
