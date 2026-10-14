namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient license seat audit.</summary>
public sealed record LenientLicenseSeatAuditView(string Code, string Title, string UpdatedAt, string Budget, string Source, string FloorValue, string LabelText, string StatusLabel)
{
    public static LenientLicenseSeatAuditView ToLenientLicenseSeatAuditView(LenientLicenseSeatAudit item)
    {
        return new LenientLicenseSeatAuditView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLicenseSeatAuditView> ToLenientLicenseSeatAuditViews(List<LenientLicenseSeatAudit> items)
    {
        var views = new List<LenientLicenseSeatAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLicenseSeatAuditView(item));
        }
        return views;
    }

    public static double SumLenientLicenseSeatAuditView(LenientLicenseSeatAuditView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedBudget + parsedFloorValue;
    }
}
