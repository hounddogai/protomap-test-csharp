namespace Acme.Workforce.Orgdesign.Span.Policy;

using System.Globalization;

/// <summary>How screens and exports show a global span rollup.</summary>
public sealed record GlobalSpanRollupView(string CodeText, string LabelText, string RevisionText, string Quantity, string Reviewer, string Active, string Approved, string Channel, string StatusLabel)
{
    public static GlobalSpanRollupView ToGlobalSpanRollupView(GlobalSpanRollup item)
    {
        return new GlobalSpanRollupView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Active}",
            $"{item.Approved}",
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSpanRollupView> ToGlobalSpanRollupViews(List<GlobalSpanRollup> items)
    {
        var views = new List<GlobalSpanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSpanRollupView(item));
        }
        return views;
    }

    public static double SumGlobalSpanRollupView(GlobalSpanRollupView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedQuantity;
    }
}
