namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle assignment.</summary>
public sealed record MeritCycleAssignmentView(string Code, string Name, string SourceText, string LocationText, string ExpiresAt, string Quantity, string PhoneText, string Visible, string StatusLabel)
{
    public static MeritCycleAssignmentView ToMeritCycleAssignmentView(MeritCycleAssignment item)
    {
        return new MeritCycleAssignmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.Source,
            item.Location,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleAssignmentView> ToMeritCycleAssignmentViews(List<MeritCycleAssignment> items)
    {
        var views = new List<MeritCycleAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleAssignmentView(item));
        }
        return views;
    }

    public static double SumMeritCycleAssignmentView(MeritCycleAssignmentView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity;
    }
}
