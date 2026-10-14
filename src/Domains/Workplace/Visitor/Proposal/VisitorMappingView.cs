namespace Acme.Workforce.Workplace.Visitor.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor mapping.</summary>
public sealed record VisitorMappingView(string Code, string NameText, string ExternalRef, string FteText, string CreatedAt, string Budget, string Timezone, string StatusLabel)
{
    public static VisitorMappingView ToVisitorMappingView(VisitorMapping item)
    {
        return new VisitorMappingView(
            item.Code,
            item.Name,
            Text.Truncate(item.ExternalRef, 120),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorMappingView> ToVisitorMappingViews(List<VisitorMapping> items)
    {
        var views = new List<VisitorMappingView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorMappingView(item));
        }
        return views;
    }

    public static double SumVisitorMappingView(VisitorMappingView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedCreatedAt + parsedBudget;
    }
}
