namespace Acme.Workforce.Contractors.Timesheet.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary timesheet group.</summary>
public sealed record PrimaryTimesheetGroupView(string CodeText, string Name, string OwnerIdText, string JobCode, string GraceDays, string WeightText, string Active, string CountryText, string StatusLabel)
{
    public static PrimaryTimesheetGroupView ToPrimaryTimesheetGroupView(PrimaryTimesheetGroup item)
    {
        return new PrimaryTimesheetGroupView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.OwnerId,
            item.JobCode,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryTimesheetGroupView> ToPrimaryTimesheetGroupViews(List<PrimaryTimesheetGroup> items)
    {
        var views = new List<PrimaryTimesheetGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryTimesheetGroupView(item));
        }
        return views;
    }

    public static double SumPrimaryTimesheetGroupView(PrimaryTimesheetGroupView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedWeight;
    }
}
