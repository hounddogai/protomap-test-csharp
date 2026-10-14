namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using System.Globalization;

/// <summary>How screens and exports show a access badge cycle.</summary>
public sealed record AccessBadgeCycleView(string CodeText, string TitleText, string Score, string EmployeeId, string CurrencyText, string Summary, string StatusLabel)
{
    public static AccessBadgeCycleView ToAccessBadgeCycleView(AccessBadgeCycle item)
    {
        return new AccessBadgeCycleView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Currency,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeCycleView> ToAccessBadgeCycleViews(List<AccessBadgeCycle> items)
    {
        var views = new List<AccessBadgeCycleView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeCycleView(item));
        }
        return views;
    }

    public static double SumAccessBadgeCycleView(AccessBadgeCycleView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
