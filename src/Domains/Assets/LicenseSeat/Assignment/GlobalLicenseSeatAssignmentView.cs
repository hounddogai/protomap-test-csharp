namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a global license seat assignment.</summary>
public sealed record GlobalLicenseSeatAssignmentView(string Code, string NameText, string MinimumText, string EmployeeIdText, string Department, string StatusLabel)
{
    public static GlobalLicenseSeatAssignmentView ToGlobalLicenseSeatAssignmentView(GlobalLicenseSeatAssignment item)
    {
        return new GlobalLicenseSeatAssignmentView(
            item.Code,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalLicenseSeatAssignmentView> ToGlobalLicenseSeatAssignmentViews(List<GlobalLicenseSeatAssignment> items)
    {
        var views = new List<GlobalLicenseSeatAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalLicenseSeatAssignmentView(item));
        }
        return views;
    }

    public static double SumGlobalLicenseSeatAssignmentView(GlobalLicenseSeatAssignmentView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
