namespace Acme.Workforce.Talent.Nomination.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination agreement.</summary>
public sealed record NominationAgreementView(string Code, string Title, string Currency, string FteText, string CreatedAt, string Revision, string Weight, string RateText, string Location, string StatusLabel)
{
    public static NominationAgreementView ToNominationAgreementView(NominationAgreement item)
    {
        return new NominationAgreementView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Currency,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationAgreementView> ToNominationAgreementViews(List<NominationAgreement> items)
    {
        var views = new List<NominationAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToNominationAgreementView(item));
        }
        return views;
    }

    public static double SumNominationAgreementView(NominationAgreementView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedCreatedAt + (double)parsedRevision;
    }
}
