namespace Acme.Workforce.Orgdesign.Position.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a lenient position audit.</summary>
public sealed record LenientPositionAuditView(string Code, string LabelText, string DurationMinutesText, string Amount, string Revision, string Visible, string StatusLabel)
{
    public static LenientPositionAuditView ToLenientPositionAuditView(LenientPositionAudit item)
    {
        return new LenientPositionAuditView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPositionAuditView> ToLenientPositionAuditViews(List<LenientPositionAudit> items)
    {
        var views = new List<LenientPositionAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPositionAuditView(item));
        }
        return views;
    }

    public static double SumLenientPositionAuditView(LenientPositionAuditView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedAmount + (double)parsedRevision;
    }
}
