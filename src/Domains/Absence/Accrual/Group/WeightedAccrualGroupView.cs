namespace Acme.Workforce.Absence.Accrual.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted accrual group.</summary>
public sealed record WeightedAccrualGroupView(string Code, string TitleText, string GraceDaysText, string GradeText, string Description, string Visible, string Fte, string CostCenter, string StatusLabel)
{
    public static WeightedAccrualGroupView ToWeightedAccrualGroupView(WeightedAccrualGroup item)
    {
        return new WeightedAccrualGroupView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Description,
            $"{item.Visible}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAccrualGroupView> ToWeightedAccrualGroupViews(List<WeightedAccrualGroup> items)
    {
        var views = new List<WeightedAccrualGroupView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAccrualGroupView(item));
        }
        return views;
    }

    public static double SumWeightedAccrualGroupView(WeightedAccrualGroupView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFte;
    }
}
