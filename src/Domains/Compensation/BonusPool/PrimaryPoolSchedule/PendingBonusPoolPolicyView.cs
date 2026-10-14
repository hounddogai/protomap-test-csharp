namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using System.Globalization;

/// <summary>How screens and exports show a pending bonus pool policy.</summary>
public sealed record PendingBonusPoolPolicyView(string Code, string TitleText, string Unit, string Ratio, string SalaryBand, string CategoryText, string ActiveText, string StatusLabel)
{
    public static PendingBonusPoolPolicyView ToPendingBonusPoolPolicyView(PendingBonusPoolPolicy item)
    {
        return new PendingBonusPoolPolicyView(
            item.Code,
            item.Title,
            item.Unit,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Category,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingBonusPoolPolicyView> ToPendingBonusPoolPolicyViews(List<PendingBonusPoolPolicy> items)
    {
        var views = new List<PendingBonusPoolPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPendingBonusPoolPolicyView(item));
        }
        return views;
    }

    public static double SumPendingBonusPoolPolicyView(PendingBonusPoolPolicyView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
