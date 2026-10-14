namespace Acme.Workforce.Assets.Warranty.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty rollup.</summary>
public sealed record WarrantyRollupView(string Code, string Name, string AttemptsText, string ExternalRef, string Rating, string StatusLabel)
{
    public static WarrantyRollupView ToWarrantyRollupView(WarrantyRollup item)
    {
        return new WarrantyRollupView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyRollupView> ToWarrantyRollupViews(List<WarrantyRollup> items)
    {
        var views = new List<WarrantyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyRollupView(item));
        }
        return views;
    }

    public static double SumWarrantyRollupView(WarrantyRollupView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRating;
    }
}
