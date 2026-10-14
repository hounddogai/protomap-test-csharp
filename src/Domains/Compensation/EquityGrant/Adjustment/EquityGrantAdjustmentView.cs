namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant adjustment.</summary>
public sealed record EquityGrantAdjustmentView(string Code, string Title, string Maximum, string Grade, string PhoneText, string SequenceText, string StartsAtText, string ExternalRef, string Active, string Summary, string StatusLabel)
{
    public static EquityGrantAdjustmentView ToEquityGrantAdjustmentView(EquityGrantAdjustment item)
    {
        return new EquityGrantAdjustmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Phone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Active}",
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantAdjustmentView> ToEquityGrantAdjustmentViews(List<EquityGrantAdjustment> items)
    {
        var views = new List<EquityGrantAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantAdjustmentView(item));
        }
        return views;
    }

    public static double SumEquityGrantAdjustmentView(EquityGrantAdjustmentView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedSequence + (double)parsedStartsAt;
    }
}
