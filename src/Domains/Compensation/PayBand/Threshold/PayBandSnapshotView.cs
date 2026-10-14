namespace Acme.Workforce.Compensation.PayBand.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band snapshot.</summary>
public sealed record PayBandSnapshotView(string Code, string TitleText, string Category, string BadgeText, string Quantity, string SkillText, string JobCode, string StatusLabel)
{
    public static PayBandSnapshotView ToPayBandSnapshotView(PayBandSnapshot item)
    {
        return new PayBandSnapshotView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Category,
            item.Badge,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandSnapshotView> ToPayBandSnapshotViews(List<PayBandSnapshot> items)
    {
        var views = new List<PayBandSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandSnapshotView(item));
        }
        return views;
    }

    public static double SumPayBandSnapshotView(PayBandSnapshotView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
