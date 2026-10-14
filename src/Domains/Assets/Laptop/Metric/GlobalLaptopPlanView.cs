namespace Acme.Workforce.Assets.Laptop.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global laptop plan.</summary>
public sealed record GlobalLaptopPlanView(string Code, string Label, string SegmentText, string Channel, string ContactEmail, string Skill, string Grade, string Amount, string StatusLabel)
{
    public static GlobalLaptopPlanView ToGlobalLaptopPlanView(GlobalLaptopPlan item)
    {
        return new GlobalLaptopPlanView(
            item.Code,
            item.Label,
            item.Segment,
            Text.Truncate(item.Channel, 64),
            Text.Truncate(item.ContactEmail, 120),
            item.Skill,
            item.Grade,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalLaptopPlanView> ToGlobalLaptopPlanViews(List<GlobalLaptopPlan> items)
    {
        var views = new List<GlobalLaptopPlanView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalLaptopPlanView(item));
        }
        return views;
    }

    public static double SumGlobalLaptopPlanView(GlobalLaptopPlanView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
