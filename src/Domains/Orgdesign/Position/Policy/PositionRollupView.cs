namespace Acme.Workforce.Orgdesign.Position.Policy;

using System.Globalization;

/// <summary>How screens and exports show a position rollup.</summary>
public sealed record PositionRollupView(string Code, string Title, string Quantity, string Skill, string SourceText, string Timezone, string GradeText, string MaximumText, string RetriesText, string StatusLabel)
{
    public static PositionRollupView ToPositionRollupView(PositionRollup item)
    {
        return new PositionRollupView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Source,
            item.Timezone,
            item.Grade,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionRollupView> ToPositionRollupViews(List<PositionRollup> items)
    {
        var views = new List<PositionRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPositionRollupView(item));
        }
        return views;
    }

    public static double SumPositionRollupView(PositionRollupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedMaximum + (double)parsedRetries;
    }
}
