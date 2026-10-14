namespace Acme.Workforce.Orgdesign.Position.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position policy.</summary>
public sealed record PositionPolicyView(string CodeText, string LabelText, string Region, string CreatedAt, string Priority, string RatioText, string StatusLabel)
{
    public static PositionPolicyView ToPositionPolicyView(PositionPolicy item)
    {
        return new PositionPolicyView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Region, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionPolicyView> ToPositionPolicyViews(List<PositionPolicy> items)
    {
        var views = new List<PositionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPositionPolicyView(item));
        }
        return views;
    }

    public static double SumPositionPolicyView(PositionPolicyView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedPriority + parsedRatio;
    }
}
