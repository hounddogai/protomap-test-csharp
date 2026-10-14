namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan bundle.</summary>
public sealed record ReturnPlanBundleView(string Code, string NameText, string MinimumText, string Archived, string Source, string ContactEmail, string Badge, string Fte, string Budget, string StatusLabel)
{
    public static ReturnPlanBundleView ToReturnPlanBundleView(ReturnPlanBundle item)
    {
        return new ReturnPlanBundleView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Source, 120),
            item.ContactEmail,
            item.Badge,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanBundleView> ToReturnPlanBundleViews(List<ReturnPlanBundle> items)
    {
        var views = new List<ReturnPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanBundleView(item));
        }
        return views;
    }

    public static double SumReturnPlanBundleView(ReturnPlanBundleView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFte + parsedBudget;
    }
}
