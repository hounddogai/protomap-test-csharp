namespace Acme.Workforce.Assets.Assignment.Policy;

using System.Globalization;

/// <summary>How screens and exports show a weighted assignment audit.</summary>
public sealed record WeightedAssignmentAuditView(string Code, string Title, string SalaryBand, string RatingText, string CapacityText, string VisibleText, string Amount, string EndsAtText, string StatusLabel)
{
    public static WeightedAssignmentAuditView ToWeightedAssignmentAuditView(WeightedAssignmentAudit item)
    {
        return new WeightedAssignmentAuditView(
            item.Code,
            item.Title,
            item.SalaryBand,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAssignmentAuditView> ToWeightedAssignmentAuditViews(List<WeightedAssignmentAudit> items)
    {
        var views = new List<WeightedAssignmentAuditView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAssignmentAuditView(item));
        }
        return views;
    }

    public static double SumWeightedAssignmentAuditView(WeightedAssignmentAuditView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCapacity + parsedAmount;
    }
}
