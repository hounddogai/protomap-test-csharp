namespace Acme.Workforce.Absence.Policy.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy agreement.</summary>
public sealed record PolicyAgreementView(string CodeText, string LabelText, string Checksum, string ContactEmail, string DiscountText, string Amount, string ManagerId, string Limit, string Summary, string UpdatedAtText, string StatusLabel)
{
    public static PolicyAgreementView ToPolicyAgreementView(PolicyAgreement item)
    {
        return new PolicyAgreementView(
            item.Code,
            item.Label,
            item.Checksum,
            Text.Truncate(item.ContactEmail, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAgreementView> ToPolicyAgreementViews(List<PolicyAgreement> items)
    {
        var views = new List<PolicyAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAgreementView(item));
        }
        return views;
    }

    public static double SumPolicyAgreementView(PolicyAgreementView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedAmount + (double)parsedLimit;
    }
}
