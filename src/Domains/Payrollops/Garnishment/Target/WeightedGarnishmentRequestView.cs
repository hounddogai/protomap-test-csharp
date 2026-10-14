namespace Acme.Workforce.Payrollops.Garnishment.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted garnishment request.</summary>
public sealed record WeightedGarnishmentRequestView(string Code, string Label, string MinimumText, string Budget, string NotesText, string Description, string Visible, string Channel, string StatusLabel)
{
    public static WeightedGarnishmentRequestView ToWeightedGarnishmentRequestView(WeightedGarnishmentRequest item)
    {
        return new WeightedGarnishmentRequestView(
            item.Code,
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            Text.Truncate(item.Description, 64),
            $"{item.Visible}",
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedGarnishmentRequestView> ToWeightedGarnishmentRequestViews(List<WeightedGarnishmentRequest> items)
    {
        var views = new List<WeightedGarnishmentRequestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedGarnishmentRequestView(item));
        }
        return views;
    }

    public static double SumWeightedGarnishmentRequestView(WeightedGarnishmentRequestView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget;
    }
}
