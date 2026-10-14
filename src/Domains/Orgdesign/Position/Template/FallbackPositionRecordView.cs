namespace Acme.Workforce.Orgdesign.Position.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback position record.</summary>
public sealed record FallbackPositionRecordView(string Code, string Label, string Notes, string FloorValue, string Skill, string Category, string StatusLabel)
{
    public static FallbackPositionRecordView ToFallbackPositionRecordView(FallbackPositionRecord item)
    {
        return new FallbackPositionRecordView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Notes, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPositionRecordView> ToFallbackPositionRecordViews(List<FallbackPositionRecord> items)
    {
        var views = new List<FallbackPositionRecordView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPositionRecordView(item));
        }
        return views;
    }

    public static double SumFallbackPositionRecordView(FallbackPositionRecordView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
