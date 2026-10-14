namespace Acme.Workforce.Talent.Pool.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool policy.</summary>
public sealed record PoolPolicyView(string Code, string Name, string DepartmentText, string OwnerId, string CapacityText, string StatusLabel)
{
    public static PoolPolicyView ToPoolPolicyView(PoolPolicy item)
    {
        return new PoolPolicyView(
            item.Code,
            item.Name,
            Text.Truncate(item.Department, 64),
            Text.Truncate(item.OwnerId, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolPolicyView> ToPoolPolicyViews(List<PoolPolicy> items)
    {
        var views = new List<PoolPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPoolPolicyView(item));
        }
        return views;
    }

    public static double SumPoolPolicyView(PoolPolicyView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
