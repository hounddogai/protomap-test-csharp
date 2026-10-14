namespace Acme.Workforce.Orgdesign.Position.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position template.</summary>
public sealed record PositionTemplateView(string Code, string Name, string Score, string Ceiling, string GraceDays, string Country, string Discount, string LabelText, string StatusLabel)
{
    public static PositionTemplateView ToPositionTemplateView(PositionTemplate item)
    {
        return new PositionTemplateView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionTemplateView> ToPositionTemplateViews(List<PositionTemplate> items)
    {
        var views = new List<PositionTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPositionTemplateView(item));
        }
        return views;
    }

    public static double SumPositionTemplateView(PositionTemplateView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedScore + parsedCeiling + (double)parsedGraceDays;
    }
}
