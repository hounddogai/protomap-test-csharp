namespace Acme.Workforce.Assets.Assignment.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default assignment snapshot.</summary>
public sealed record DefaultAssignmentSnapshotView(string Code, string LabelText, string Attempts, string DepartmentText, string GraceDays, string Discount, string ContactEmail, string EmployeeIdText, string Score, string DescriptionText, string StatusLabel)
{
    public static DefaultAssignmentSnapshotView ToDefaultAssignmentSnapshotView(DefaultAssignmentSnapshot item)
    {
        return new DefaultAssignmentSnapshotView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAssignmentSnapshotView> ToDefaultAssignmentSnapshotViews(List<DefaultAssignmentSnapshot> items)
    {
        var views = new List<DefaultAssignmentSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAssignmentSnapshotView(item));
        }
        return views;
    }

    public static double SumDefaultAssignmentSnapshotView(DefaultAssignmentSnapshotView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedGraceDays + parsedDiscount;
    }
}
