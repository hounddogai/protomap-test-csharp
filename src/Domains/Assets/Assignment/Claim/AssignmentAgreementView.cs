namespace Acme.Workforce.Assets.Assignment.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment agreement.</summary>
public sealed record AssignmentAgreementView(string Code, string Title, string Cost, string EmployeeIdText, string Phone, string Rating, string Rate, string Active, string Summary, string StatusLabel)
{
    public static AssignmentAgreementView ToAssignmentAgreementView(AssignmentAgreement item)
    {
        return new AssignmentAgreementView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Phone, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentAgreementView> ToAssignmentAgreementViews(List<AssignmentAgreement> items)
    {
        var views = new List<AssignmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentAgreementView(item));
        }
        return views;
    }

    public static double SumAssignmentAgreementView(AssignmentAgreementView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedCost + parsedRating + parsedRate;
    }
}
