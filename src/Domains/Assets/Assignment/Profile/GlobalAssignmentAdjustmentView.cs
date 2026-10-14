namespace Acme.Workforce.Assets.Assignment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global assignment adjustment.</summary>
public sealed record GlobalAssignmentAdjustmentView(string CodeText, string LabelText, string TenureMonths, string ExpiresAt, string Amount, string Active, string Skill, string CostCenter, string Priority, string StatusLabel)
{
    public static GlobalAssignmentAdjustmentView ToGlobalAssignmentAdjustmentView(GlobalAssignmentAdjustment item)
    {
        return new GlobalAssignmentAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Skill,
            item.CostCenter,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAssignmentAdjustmentView> ToGlobalAssignmentAdjustmentViews(List<GlobalAssignmentAdjustment> items)
    {
        var views = new List<GlobalAssignmentAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAssignmentAdjustmentView(item));
        }
        return views;
    }

    public static double SumGlobalAssignmentAdjustmentView(GlobalAssignmentAdjustmentView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedExpiresAt + parsedAmount;
    }
}
