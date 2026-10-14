namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a capped leave request template.</summary>
public sealed record CappedLeaveRequestTemplateView(string CodeText, string Label, string Department, string Description, string TimezoneText, string Minimum, string StatusLabel)
{
    public static CappedLeaveRequestTemplateView ToCappedLeaveRequestTemplateView(CappedLeaveRequestTemplate item)
    {
        return new CappedLeaveRequestTemplateView(
            item.Code,
            item.Label,
            item.Department,
            item.Description,
            item.Timezone,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLeaveRequestTemplateView> ToCappedLeaveRequestTemplateViews(List<CappedLeaveRequestTemplate> items)
    {
        var views = new List<CappedLeaveRequestTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLeaveRequestTemplateView(item));
        }
        return views;
    }

    public static double SumCappedLeaveRequestTemplateView(CappedLeaveRequestTemplateView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
