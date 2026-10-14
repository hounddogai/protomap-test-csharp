namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted license seat bundle.</summary>
public sealed record WeightedLicenseSeatBundleView(string Code, string Label, string Revision, string FloorValue, string Budget, string UnitText, string WindowDays, string CapacityText, string GradeText, string Minimum, string EmployeeId, string StatusLabel)
{
    public static WeightedLicenseSeatBundleView ToWeightedLicenseSeatBundleView(WeightedLicenseSeatBundle item)
    {
        return new WeightedLicenseSeatBundleView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedLicenseSeatBundleView> ToWeightedLicenseSeatBundleViews(List<WeightedLicenseSeatBundle> items)
    {
        var views = new List<WeightedLicenseSeatBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedLicenseSeatBundleView(item));
        }
        return views;
    }

    public static double SumWeightedLicenseSeatBundleView(WeightedLicenseSeatBundleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedFloorValue + parsedBudget;
    }
}
