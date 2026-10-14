namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model entry.</summary>
public sealed record CostModelEntryView(string Code, string Name, string ChecksumText, string Region, string RatioText, string DepartmentText, string OwnerId, string ApprovedText, string Quantity, string Channel, string StatusLabel)
{
    public static CostModelEntryView ToCostModelEntryView(CostModelEntry item)
    {
        return new CostModelEntryView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Checksum, 80),
            item.Region,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.OwnerId, 80),
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelEntryView> ToCostModelEntryViews(List<CostModelEntry> items)
    {
        var views = new List<CostModelEntryView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelEntryView(item));
        }
        return views;
    }

    public static double SumCostModelEntryView(CostModelEntryView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedQuantity;
    }
}
