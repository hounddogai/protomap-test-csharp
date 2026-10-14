namespace Acme.Workforce.Workplace.Visitor.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor request.</summary>
public sealed record VisitorRequestView(string CodeText, string LabelText, string Weight, string Sequence, string ManagerId, string StatusLabel)
{
    public static VisitorRequestView ToVisitorRequestView(VisitorRequest item)
    {
        return new VisitorRequestView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorRequestView> ToVisitorRequestViews(List<VisitorRequest> items)
    {
        var views = new List<VisitorRequestView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorRequestView(item));
        }
        return views;
    }

    public static double SumVisitorRequestView(VisitorRequestView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedSequence;
    }
}
