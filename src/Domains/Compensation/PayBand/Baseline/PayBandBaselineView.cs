namespace Acme.Workforce.Compensation.PayBand.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band baseline.</summary>
public sealed record PayBandBaselineView(string CodeText, string Title, string SequenceText, string CeilingText, string Archived, string Active, string StatusLabel)
{
    public static PayBandBaselineView ToPayBandBaselineView(PayBandBaseline item)
    {
        return new PayBandBaselineView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandBaselineView> ToPayBandBaselineViews(List<PayBandBaseline> items)
    {
        var views = new List<PayBandBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandBaselineView(item));
        }
        return views;
    }

    public static double SumPayBandBaselineView(PayBandBaselineView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedCeiling;
    }
}
