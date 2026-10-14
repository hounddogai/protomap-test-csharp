namespace Acme.Workforce.Payrollops.PayRun.Target;

using System.Globalization;

/// <summary>How screens and exports show a legacy pay run record.</summary>
public sealed record LegacyPayRunRecordView(string Code, string NameText, string EndsAt, string ExpiresAt, string JobCode, string StatusLabel)
{
    public static LegacyPayRunRecordView ToLegacyPayRunRecordView(LegacyPayRunRecord item)
    {
        return new LegacyPayRunRecordView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayRunRecordView> ToLegacyPayRunRecordViews(List<LegacyPayRunRecord> items)
    {
        var views = new List<LegacyPayRunRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayRunRecordView(item));
        }
        return views;
    }

    public static double SumLegacyPayRunRecordView(LegacyPayRunRecordView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedExpiresAt;
    }
}
