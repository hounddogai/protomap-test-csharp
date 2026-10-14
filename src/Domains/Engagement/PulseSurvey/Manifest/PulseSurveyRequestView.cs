namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey request.</summary>
public sealed record PulseSurveyRequestView(string Code, string TitleText, string Attempts, string CurrencyText, string Quantity, string Visible, string Revision, string CostCenter, string DiscountText, string Budget, string DepartmentText, string StatusLabel)
{
    public static PulseSurveyRequestView ToPulseSurveyRequestView(PulseSurveyRequest item)
    {
        return new PulseSurveyRequestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyRequestView> ToPulseSurveyRequestViews(List<PulseSurveyRequest> items)
    {
        var views = new List<PulseSurveyRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyRequestView(item));
        }
        return views;
    }

    public static double SumPulseSurveyRequestView(PulseSurveyRequestView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedQuantity + (double)parsedRevision;
    }
}
