namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay assignment.</summary>
public sealed record RetroPayAssignmentView(string CodeText, string Label, string OwnerId, string Reviewer, string ExternalRef, string QuantityText, string StatusLabel)
{
    public static RetroPayAssignmentView ToRetroPayAssignmentView(RetroPayAssignment item)
    {
        return new RetroPayAssignmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.OwnerId, 40),
            item.Reviewer,
            Text.Truncate(item.ExternalRef, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayAssignmentView> ToRetroPayAssignmentViews(List<RetroPayAssignment> items)
    {
        var views = new List<RetroPayAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayAssignmentView(item));
        }
        return views;
    }

    public static double SumRetroPayAssignmentView(RetroPayAssignmentView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
