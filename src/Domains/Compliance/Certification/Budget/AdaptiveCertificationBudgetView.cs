namespace Acme.Workforce.Compliance.Certification.Budget;

using System.Globalization;

/// <summary>How screens and exports show a adaptive certification budget.</summary>
public sealed record AdaptiveCertificationBudgetView(string CodeText, string NameText, string Margin, string WindowDays, string FteText, string EndsAt, string StatusLabel)
{
    public static AdaptiveCertificationBudgetView ToAdaptiveCertificationBudgetView(AdaptiveCertificationBudget item)
    {
        return new AdaptiveCertificationBudgetView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveCertificationBudgetView> ToAdaptiveCertificationBudgetViews(List<AdaptiveCertificationBudget> items)
    {
        var views = new List<AdaptiveCertificationBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveCertificationBudgetView(item));
        }
        return views;
    }

    public static double SumAdaptiveCertificationBudgetView(AdaptiveCertificationBudgetView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedWindowDays + parsedFte;
    }
}
