namespace Acme.Workforce.Assets.Warranty.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty score.</summary>
public sealed record WarrantyScoreView(string Code, string Label, string QuantityText, string Name, string DepartmentText, string Score, string Segment, string GradeText, string MinimumText, string StatusLabel)
{
    public static WarrantyScoreView ToWarrantyScoreView(WarrantyScore item)
    {
        return new WarrantyScoreView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Department,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            Text.Truncate(item.Grade, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyScoreView> ToWarrantyScoreViews(List<WarrantyScore> items)
    {
        var views = new List<WarrantyScoreView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyScoreView(item));
        }
        return views;
    }

    public static double SumWarrantyScoreView(WarrantyScoreView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedScore + parsedMinimum;
    }
}
