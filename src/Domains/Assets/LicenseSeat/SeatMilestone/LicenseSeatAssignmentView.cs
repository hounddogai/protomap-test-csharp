namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat assignment.</summary>
public sealed record LicenseSeatAssignmentView(string Code, string LabelText, string MarginText, string Minimum, string UpdatedAt, string GradeText, string Locale, string Source, string StatusLabel)
{
    public static LicenseSeatAssignmentView ToLicenseSeatAssignmentView(LicenseSeatAssignment item)
    {
        return new LicenseSeatAssignmentView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Locale,
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatAssignmentView> ToLicenseSeatAssignmentViews(List<LicenseSeatAssignment> items)
    {
        var views = new List<LicenseSeatAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatAssignmentView(item));
        }
        return views;
    }

    public static double SumLicenseSeatAssignmentView(LicenseSeatAssignmentView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMargin + parsedMinimum + (double)parsedUpdatedAt;
    }
}
