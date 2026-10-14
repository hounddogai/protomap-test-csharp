namespace Acme.Workforce.Compensation.PayBand.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band score.</summary>
public sealed record PayBandScoreView(string Code, string Label, string Revision, string Category, string Title, string Unit, string Quantity, string Name, string Minimum, string Approved, string StatusLabel)
{
    public static PayBandScoreView ToPayBandScoreView(PayBandScore item)
    {
        return new PayBandScoreView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.Title,
            item.Unit,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandScoreView> ToPayBandScoreViews(List<PayBandScore> items)
    {
        var views = new List<PayBandScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandScoreView(item));
        }
        return views;
    }

    public static double SumPayBandScoreView(PayBandScoreView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedQuantity + parsedMinimum;
    }
}
