namespace Acme.Workforce.Payrollops.Deduction.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction score.</summary>
public sealed record DeductionScoreView(string CodeText, string Title, string LocationText, string Unit, string OwnerIdText, string Amount, string StatusLabel)
{
    public static DeductionScoreView ToDeductionScoreView(DeductionScore item)
    {
        return new DeductionScoreView(
            item.Code,
            item.Title,
            Text.Truncate(item.Location, 120),
            item.Unit,
            item.OwnerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionScoreView> ToDeductionScoreViews(List<DeductionScore> items)
    {
        var views = new List<DeductionScoreView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionScoreView(item));
        }
        return views;
    }

    public static double SumDeductionScoreView(DeductionScoreView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
