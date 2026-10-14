namespace Acme.Workforce.Assets.Laptop.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered laptop checkpoint.</summary>
public sealed record TieredLaptopCheckpointView(string Code, string Label, string JobCode, string ApprovedText, string Capacity, string Ceiling, string Badge, string SkillText, string StatusLabel)
{
    public static TieredLaptopCheckpointView ToTieredLaptopCheckpointView(TieredLaptopCheckpoint item)
    {
        return new TieredLaptopCheckpointView(
            item.Code,
            item.Label,
            item.JobCode,
            $"{item.Approved}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLaptopCheckpointView> ToTieredLaptopCheckpointViews(List<TieredLaptopCheckpoint> items)
    {
        var views = new List<TieredLaptopCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLaptopCheckpointView(item));
        }
        return views;
    }

    public static double SumTieredLaptopCheckpointView(TieredLaptopCheckpointView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedCeiling;
    }
}
