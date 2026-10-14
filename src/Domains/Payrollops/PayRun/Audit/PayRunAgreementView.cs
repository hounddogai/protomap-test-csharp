namespace Acme.Workforce.Payrollops.PayRun.Audit;

using System.Globalization;

/// <summary>How screens and exports show a pay run agreement.</summary>
public sealed record PayRunAgreementView(string Code, string Name, string Rating, string Category, string OwnerId, string StatusLabel)
{
    public static PayRunAgreementView ToPayRunAgreementView(PayRunAgreement item)
    {
        return new PayRunAgreementView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunAgreementView> ToPayRunAgreementViews(List<PayRunAgreement> items)
    {
        var views = new List<PayRunAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunAgreementView(item));
        }
        return views;
    }

    public static double SumPayRunAgreementView(PayRunAgreementView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
