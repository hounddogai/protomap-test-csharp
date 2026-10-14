namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using System.Globalization;

/// <summary>How screens and exports show a rolling balance group.</summary>
public sealed record RollingBalanceGroupView(string Code, string Label, string Ratio, string Sequence, string UpdatedAt, string StatusLabel)
{
    public static RollingBalanceGroupView ToRollingBalanceGroupView(RollingBalanceGroup item)
    {
        return new RollingBalanceGroupView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBalanceGroupView> ToRollingBalanceGroupViews(List<RollingBalanceGroup> items)
    {
        var views = new List<RollingBalanceGroupView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBalanceGroupView(item));
        }
        return views;
    }

    public static double SumRollingBalanceGroupView(RollingBalanceGroupView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedSequence + (double)parsedUpdatedAt;
    }
}
