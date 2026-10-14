namespace Acme.Workforce.Orgdesign.Headcount.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default headcount profile.</summary>
public sealed record DefaultHeadcountProfileView(string Code, string Label, string BudgetText, string Sequence, string Category, string Grade, string StatusLabel)
{
    public static DefaultHeadcountProfileView ToDefaultHeadcountProfileView(DefaultHeadcountProfile item)
    {
        return new DefaultHeadcountProfileView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHeadcountProfileView> ToDefaultHeadcountProfileViews(List<DefaultHeadcountProfile> items)
    {
        var views = new List<DefaultHeadcountProfileView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHeadcountProfileView(item));
        }
        return views;
    }

    public static double SumDefaultHeadcountProfileView(DefaultHeadcountProfileView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedSequence;
    }
}
