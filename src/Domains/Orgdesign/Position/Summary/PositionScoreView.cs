namespace Acme.Workforce.Orgdesign.Position.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position score.</summary>
public sealed record PositionScoreView(string Code, string Name, string Retries, string Title, string QuantityText, string Description, string EmployeeId, string StatusLabel)
{
    public static PositionScoreView ToPositionScoreView(PositionScore item)
    {
        return new PositionScoreView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionScoreView> ToPositionScoreViews(List<PositionScore> items)
    {
        var views = new List<PositionScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPositionScoreView(item));
        }
        return views;
    }

    public static double SumPositionScoreView(PositionScoreView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedQuantity;
    }
}
