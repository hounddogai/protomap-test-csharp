namespace Acme.Workforce.Workplace.Visitor.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor checkpoint.</summary>
public sealed record VisitorCheckpointView(string CodeText, string Title, string Skill, string TenureMonthsText, string Quantity, string StatusLabel)
{
    public static VisitorCheckpointView ToVisitorCheckpointView(VisitorCheckpoint item)
    {
        return new VisitorCheckpointView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Skill,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorCheckpointView> ToVisitorCheckpointViews(List<VisitorCheckpoint> items)
    {
        var views = new List<VisitorCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorCheckpointView(item));
        }
        return views;
    }

    public static double SumVisitorCheckpointView(VisitorCheckpointView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedQuantity;
    }
}
