namespace Acme.Workforce.Talent.CareerPath.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path plan.</summary>
public sealed record CareerPathPlanView(string CodeText, string Label, string Category, string CostCenter, string Locale, string TitleText, string Quantity, string StatusLabel)
{
    public static CareerPathPlanView ToCareerPathPlanView(CareerPathPlan item)
    {
        return new CareerPathPlanView(
            item.Code,
            item.Label,
            item.Category,
            Text.Truncate(item.CostCenter, 120),
            item.Locale,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathPlanView> ToCareerPathPlanViews(List<CareerPathPlan> items)
    {
        var views = new List<CareerPathPlanView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathPlanView(item));
        }
        return views;
    }

    public static double SumCareerPathPlanView(CareerPathPlanView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
