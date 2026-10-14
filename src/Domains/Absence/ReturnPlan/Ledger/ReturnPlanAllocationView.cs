namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan allocation.</summary>
public sealed record ReturnPlanAllocationView(string CodeText, string Name, string Rate, string GradeText, string LocaleText, string CostCenter, string JobCodeText, string Minimum, string Reviewer, string StatusLabel)
{
    public static ReturnPlanAllocationView ToReturnPlanAllocationView(ReturnPlanAllocation item)
    {
        return new ReturnPlanAllocationView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            Text.Truncate(item.Locale, 120),
            item.CostCenter,
            Text.Truncate(item.JobCode, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanAllocationView> ToReturnPlanAllocationViews(List<ReturnPlanAllocation> items)
    {
        var views = new List<ReturnPlanAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanAllocationView(item));
        }
        return views;
    }

    public static double SumReturnPlanAllocationView(ReturnPlanAllocationView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedRate + parsedMinimum;
    }
}
