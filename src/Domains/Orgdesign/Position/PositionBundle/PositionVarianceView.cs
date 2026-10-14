namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using System.Globalization;

/// <summary>How screens and exports show a position variance.</summary>
public sealed record PositionVarianceView(string Code, string Label, string WindowDays, string WeightText, string RevisionText, string EndsAt, string Hours, string StatusLabel)
{
    public static PositionVarianceView ToPositionVarianceView(PositionVariance item)
    {
        return new PositionVarianceView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionVarianceView> ToPositionVarianceViews(List<PositionVariance> items)
    {
        var views = new List<PositionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPositionVarianceView(item));
        }
        return views;
    }

    public static double SumPositionVarianceView(PositionVarianceView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedWeight + (double)parsedRevision;
    }
}
