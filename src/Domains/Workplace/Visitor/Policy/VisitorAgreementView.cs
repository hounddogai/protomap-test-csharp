namespace Acme.Workforce.Workplace.Visitor.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor agreement.</summary>
public sealed record VisitorAgreementView(string CodeText, string Name, string DurationMinutes, string ContactEmail, string ExpiresAtText, string Budget, string ActiveText, string QuantityText, string LocationText, string Timezone, string StatusLabel)
{
    public static VisitorAgreementView ToVisitorAgreementView(VisitorAgreement item)
    {
        return new VisitorAgreementView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Timezone, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorAgreementView> ToVisitorAgreementViews(List<VisitorAgreement> items)
    {
        var views = new List<VisitorAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorAgreementView(item));
        }
        return views;
    }

    public static double SumVisitorAgreementView(VisitorAgreementView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedExpiresAt + parsedBudget;
    }
}
