namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled retro pay checkpoint.</summary>
public sealed record ScheduledRetroPayCheckpointView(string CodeText, string Label, string Priority, string Currency, string ApprovedText, string LocaleText, string OwnerIdText, string CostCenter, string StatusLabel)
{
    public static ScheduledRetroPayCheckpointView ToScheduledRetroPayCheckpointView(ScheduledRetroPayCheckpoint item)
    {
        return new ScheduledRetroPayCheckpointView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Approved}",
            Text.Truncate(item.Locale, 120),
            item.OwnerId,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRetroPayCheckpointView> ToScheduledRetroPayCheckpointViews(List<ScheduledRetroPayCheckpoint> items)
    {
        var views = new List<ScheduledRetroPayCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRetroPayCheckpointView(item));
        }
        return views;
    }

    public static double SumScheduledRetroPayCheckpointView(ScheduledRetroPayCheckpointView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
