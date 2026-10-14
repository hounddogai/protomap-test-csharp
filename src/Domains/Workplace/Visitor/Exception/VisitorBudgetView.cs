namespace Acme.Workforce.Workplace.Visitor.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor budget.</summary>
public sealed record VisitorBudgetView(string CodeText, string Label, string OwnerId, string Weight, string Source, string ContactEmail, string Name, string StatusLabel)
{
    public static VisitorBudgetView ToVisitorBudgetView(VisitorBudget item)
    {
        return new VisitorBudgetView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 40),
            item.OwnerId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.ContactEmail,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorBudgetView> ToVisitorBudgetViews(List<VisitorBudget> items)
    {
        var views = new List<VisitorBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorBudgetView(item));
        }
        return views;
    }

    public static double SumVisitorBudgetView(VisitorBudgetView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
