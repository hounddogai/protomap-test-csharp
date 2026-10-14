namespace Acme.Workforce.Workplace.Visitor.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor reservation.</summary>
public sealed record VisitorReservationView(string CodeText, string Title, string Capacity, string Sequence, string SalaryBand, string Timezone, string Priority, string CostCenterText, string ThresholdText, string BudgetText, string StatusLabel)
{
    public static VisitorReservationView ToVisitorReservationView(VisitorReservation item)
    {
        return new VisitorReservationView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Timezone,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorReservationView> ToVisitorReservationViews(List<VisitorReservation> items)
    {
        var views = new List<VisitorReservationView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorReservationView(item));
        }
        return views;
    }

    public static double SumVisitorReservationView(VisitorReservationView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedSequence + (double)parsedPriority;
    }
}
