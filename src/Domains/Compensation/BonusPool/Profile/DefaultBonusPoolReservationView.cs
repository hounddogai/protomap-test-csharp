namespace Acme.Workforce.Compensation.BonusPool.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default bonus pool reservation.</summary>
public sealed record DefaultBonusPoolReservationView(string Code, string Name, string BudgetText, string Ratio, string UpdatedAt, string UnitText, string CostCenter, string ActiveText, string FloorValueText, string Revision, string MinimumText, string StatusLabel)
{
    public static DefaultBonusPoolReservationView ToDefaultBonusPoolReservationView(DefaultBonusPoolReservation item)
    {
        return new DefaultBonusPoolReservationView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.CostCenter, 40),
            $"{item.Active}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBonusPoolReservationView> ToDefaultBonusPoolReservationViews(List<DefaultBonusPoolReservation> items)
    {
        var views = new List<DefaultBonusPoolReservationView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBonusPoolReservationView(item));
        }
        return views;
    }

    public static double SumDefaultBonusPoolReservationView(DefaultBonusPoolReservationView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedBudget + parsedRatio + (double)parsedUpdatedAt;
    }
}
