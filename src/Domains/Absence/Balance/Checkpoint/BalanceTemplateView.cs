namespace Acme.Workforce.Absence.Balance.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance template.</summary>
public sealed record BalanceTemplateView(string Code, string Name, string Country, string Fte, string GraceDays, string Approved, string Summary, string Reviewer, string StatusLabel)
{
    public static BalanceTemplateView ToBalanceTemplateView(BalanceTemplate item)
    {
        return new BalanceTemplateView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Country,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Summary,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceTemplateView> ToBalanceTemplateViews(List<BalanceTemplate> items)
    {
        var views = new List<BalanceTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceTemplateView(item));
        }
        return views;
    }

    public static double SumBalanceTemplateView(BalanceTemplateView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedGraceDays;
    }
}
