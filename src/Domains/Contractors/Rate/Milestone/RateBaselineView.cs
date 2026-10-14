namespace Acme.Workforce.Contractors.Rate.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate baseline.</summary>
public sealed record RateBaselineView(string CodeText, string Name, string SalaryBand, string Locale, string Visible, string Sequence, string RegionText, string StatusLabel)
{
    public static RateBaselineView ToRateBaselineView(RateBaseline item)
    {
        return new RateBaselineView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.SalaryBand,
            Text.Truncate(item.Locale, 120),
            $"{item.Visible}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateBaselineView> ToRateBaselineViews(List<RateBaseline> items)
    {
        var views = new List<RateBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToRateBaselineView(item));
        }
        return views;
    }

    public static double SumRateBaselineView(RateBaselineView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
