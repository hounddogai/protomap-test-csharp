namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant request.</summary>
public sealed record EquityGrantRequestView(string Code, string TitleText, string Name, string Amount, string StartsAt, string Notes, string CostCenter, string Attempts, string Hours, string StatusLabel)
{
    public static EquityGrantRequestView ToEquityGrantRequestView(EquityGrantRequest item)
    {
        return new EquityGrantRequestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.CostCenter,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantRequestView> ToEquityGrantRequestViews(List<EquityGrantRequest> items)
    {
        var views = new List<EquityGrantRequestView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantRequestView(item));
        }
        return views;
    }

    public static double SumEquityGrantRequestView(EquityGrantRequestView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedStartsAt + (double)parsedAttempts;
    }
}
