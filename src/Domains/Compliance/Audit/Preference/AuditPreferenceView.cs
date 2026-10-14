namespace Acme.Workforce.Compliance.Audit.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit preference.</summary>
public sealed record AuditPreferenceView(string CodeText, string LabelText, string DurationMinutesText, string CostText, string NameText, string BatchSize, string Hours, string Amount, string StatusLabel)
{
    public static AuditPreferenceView ToAuditPreferenceView(AuditPreference item)
    {
        return new AuditPreferenceView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditPreferenceView> ToAuditPreferenceViews(List<AuditPreference> items)
    {
        var views = new List<AuditPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAuditPreferenceView(item));
        }
        return views;
    }

    public static double SumAuditPreferenceView(AuditPreferenceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost + (double)parsedBatchSize;
    }
}
