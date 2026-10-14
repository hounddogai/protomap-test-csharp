namespace Acme.Workforce.Orgdesign.Span.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual span window.</summary>
public sealed record ManualSpanWindowView(string CodeText, string Title, string EmployeeId, string GraceDays, string Approved, string Revision, string Ratio, string Capacity, string DurationMinutesText, string StatusLabel)
{
    public static ManualSpanWindowView ToManualSpanWindowView(ManualSpanWindow item)
    {
        return new ManualSpanWindowView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.EmployeeId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualSpanWindowView> ToManualSpanWindowViews(List<ManualSpanWindow> items)
    {
        var views = new List<ManualSpanWindowView>();
        foreach (var item in items)
        {
            views.Add(ToManualSpanWindowView(item));
        }
        return views;
    }

    public static double SumManualSpanWindowView(ManualSpanWindowView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedRevision + parsedRatio;
    }
}
