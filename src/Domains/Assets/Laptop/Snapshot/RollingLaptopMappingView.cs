namespace Acme.Workforce.Assets.Laptop.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling laptop mapping.</summary>
public sealed record RollingLaptopMappingView(string Code, string NameText, string OwnerIdText, string SkillText, string Amount, string StatusLabel)
{
    public static RollingLaptopMappingView ToRollingLaptopMappingView(RollingLaptopMapping item)
    {
        return new RollingLaptopMappingView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 64),
            item.OwnerId,
            item.Skill,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingLaptopMappingView> ToRollingLaptopMappingViews(List<RollingLaptopMapping> items)
    {
        var views = new List<RollingLaptopMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRollingLaptopMappingView(item));
        }
        return views;
    }

    public static double SumRollingLaptopMappingView(RollingLaptopMappingView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
