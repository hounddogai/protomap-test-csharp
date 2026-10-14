namespace Acme.Workforce.Absence.Balance.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual balance cycle.</summary>
public sealed record ManualBalanceCycleView(string CodeText, string Name, string StartsAt, string EndsAt, string UnitText, string Notes, string ExternalRefText, string GradeText, string AmountText, string Department, string StatusLabel)
{
    public static ManualBalanceCycleView ToManualBalanceCycleView(ManualBalanceCycle item)
    {
        return new ManualBalanceCycleView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Notes,
            item.ExternalRef,
            item.Grade,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualBalanceCycleView> ToManualBalanceCycleViews(List<ManualBalanceCycle> items)
    {
        var views = new List<ManualBalanceCycleView>();
        foreach (var item in items)
        {
            views.Add(ToManualBalanceCycleView(item));
        }
        return views;
    }

    public static double SumManualBalanceCycleView(ManualBalanceCycleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedEndsAt + parsedAmount;
    }
}
