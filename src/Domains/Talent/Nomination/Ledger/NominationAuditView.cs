namespace Acme.Workforce.Talent.Nomination.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a nomination audit.</summary>
public sealed record NominationAuditView(string CodeText, string Name, string NotesText, string RatioText, string Approved, string TenureMonthsText, string Minimum, string MarginText, string Grade, string StatusLabel)
{
    public static NominationAuditView ToNominationAuditView(NominationAudit item)
    {
        return new NominationAuditView(
            item.Code,
            item.Name,
            item.Notes,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationAuditView> ToNominationAuditViews(List<NominationAudit> items)
    {
        var views = new List<NominationAuditView>();
        foreach (var item in items)
        {
            views.Add(ToNominationAuditView(item));
        }
        return views;
    }

    public static double SumNominationAuditView(NominationAuditView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedTenureMonths + parsedMinimum;
    }
}
