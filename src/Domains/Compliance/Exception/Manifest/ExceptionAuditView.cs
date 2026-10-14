namespace Acme.Workforce.Compliance.Exception.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception audit.</summary>
public sealed record ExceptionAuditView(string Code, string LabelText, string UpdatedAtText, string LocationText, string Region, string Title, string DurationMinutes, string OwnerId, string Priority, string MinimumText, string Discount, string StatusLabel)
{
    public static ExceptionAuditView ToExceptionAuditView(ExceptionAudit item)
    {
        return new ExceptionAuditView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Region, 120),
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionAuditView> ToExceptionAuditViews(List<ExceptionAudit> items)
    {
        var views = new List<ExceptionAuditView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionAuditView(item));
        }
        return views;
    }

    public static double SumExceptionAuditView(ExceptionAuditView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedDurationMinutes + (double)parsedPriority;
    }
}
