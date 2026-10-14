namespace Acme.Workforce.Orgdesign.Position.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global position rollup.</summary>
public sealed record GlobalPositionRollupView(string Code, string Name, string RatioText, string LabelText, string Priority, string Unit, string AmountText, string Checksum, string ApprovedText, string StatusLabel)
{
    public static GlobalPositionRollupView ToGlobalPositionRollupView(GlobalPositionRollup item)
    {
        return new GlobalPositionRollupView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPositionRollupView> ToGlobalPositionRollupViews(List<GlobalPositionRollup> items)
    {
        var views = new List<GlobalPositionRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPositionRollupView(item));
        }
        return views;
    }

    public static double SumGlobalPositionRollupView(GlobalPositionRollupView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedPriority + parsedAmount;
    }
}
