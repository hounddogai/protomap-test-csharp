namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount mapping.</summary>
public sealed record HeadcountMappingView(string Code, string Label, string CostCenter, string SequenceText, string VersionText, string Name, string Summary, string Notes, string Ceiling, string StatusLabel)
{
    public static HeadcountMappingView ToHeadcountMappingView(HeadcountMapping item)
    {
        return new HeadcountMappingView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Summary,
            Text.Truncate(item.Notes, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountMappingView> ToHeadcountMappingViews(List<HeadcountMapping> items)
    {
        var views = new List<HeadcountMappingView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountMappingView(item));
        }
        return views;
    }

    public static double SumHeadcountMappingView(HeadcountMappingView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedVersion + parsedCeiling;
    }
}
