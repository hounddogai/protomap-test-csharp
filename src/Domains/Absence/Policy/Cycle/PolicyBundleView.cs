namespace Acme.Workforce.Absence.Policy.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy bundle.</summary>
public sealed record PolicyBundleView(string CodeText, string LabelText, string CostCenter, string UnitText, string Department, string TitleText, string CapacityText, string Currency, string ContactEmail, string FloorValue, string StatusLabel)
{
    public static PolicyBundleView ToPolicyBundleView(PolicyBundle item)
    {
        return new PolicyBundleView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.Unit,
            item.Department,
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.ContactEmail, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyBundleView> ToPolicyBundleViews(List<PolicyBundle> items)
    {
        var views = new List<PolicyBundleView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyBundleView(item));
        }
        return views;
    }

    public static double SumPolicyBundleView(PolicyBundleView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFloorValue;
    }
}
