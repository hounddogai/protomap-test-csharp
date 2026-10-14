namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan profile.</summary>
public sealed record ReturnPlanProfileView(string Code, string Name, string FloorValue, string Source, string WindowDays, string StatusLabel)
{
    public static ReturnPlanProfileView ToReturnPlanProfileView(ReturnPlanProfile item)
    {
        return new ReturnPlanProfileView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanProfileView> ToReturnPlanProfileViews(List<ReturnPlanProfile> items)
    {
        var views = new List<ReturnPlanProfileView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanProfileView(item));
        }
        return views;
    }

    public static double SumReturnPlanProfileView(ReturnPlanProfileView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedWindowDays;
    }
}
