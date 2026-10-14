namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan variance.</summary>
public sealed record ReturnPlanVarianceView(string Code, string NameText, string Maximum, string FloorValueText, string Reviewer, string StatusLabel)
{
    public static ReturnPlanVarianceView ToReturnPlanVarianceView(ReturnPlanVariance item)
    {
        return new ReturnPlanVarianceView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanVarianceView> ToReturnPlanVarianceViews(List<ReturnPlanVariance> items)
    {
        var views = new List<ReturnPlanVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanVarianceView(item));
        }
        return views;
    }

    public static double SumReturnPlanVarianceView(ReturnPlanVarianceView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedFloorValue;
    }
}
