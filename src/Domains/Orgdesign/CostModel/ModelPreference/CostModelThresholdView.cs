namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model threshold.</summary>
public sealed record CostModelThresholdView(string Code, string Name, string Sequence, string Region, string PriorityText, string NotesText, string CeilingText, string StatusLabel)
{
    public static CostModelThresholdView ToCostModelThresholdView(CostModelThreshold item)
    {
        return new CostModelThresholdView(
            item.Code,
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelThresholdView> ToCostModelThresholdViews(List<CostModelThreshold> items)
    {
        var views = new List<CostModelThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelThresholdView(item));
        }
        return views;
    }

    public static double SumCostModelThresholdView(CostModelThresholdView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedPriority + parsedCeiling;
    }
}
