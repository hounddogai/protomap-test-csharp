namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using System.Globalization;

/// <summary>How screens and exports show a manual retro pay budget.</summary>
public sealed record ManualRetroPayBudgetView(string CodeText, string LabelText, string LocaleText, string Name, string Category, string OwnerId, string EndsAtText, string StatusLabel)
{
    public static ManualRetroPayBudgetView ToManualRetroPayBudgetView(ManualRetroPayBudget item)
    {
        return new ManualRetroPayBudgetView(
            item.Code,
            item.Label,
            item.Locale,
            item.Name,
            item.Category,
            item.OwnerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualRetroPayBudgetView> ToManualRetroPayBudgetViews(List<ManualRetroPayBudget> items)
    {
        var views = new List<ManualRetroPayBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToManualRetroPayBudgetView(item));
        }
        return views;
    }

    public static double SumManualRetroPayBudgetView(ManualRetroPayBudgetView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
