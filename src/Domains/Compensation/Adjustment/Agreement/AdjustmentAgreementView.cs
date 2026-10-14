namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment agreement.</summary>
public sealed record AdjustmentAgreementView(string Code, string Name, string Country, string Minimum, string Team, string PinnedText, string Checksum, string CreatedAt, string RetriesText, string GradeText, string Summary, string StatusLabel)
{
    public static AdjustmentAgreementView ToAdjustmentAgreementView(AdjustmentAgreement item)
    {
        return new AdjustmentAgreementView(
            item.Code,
            item.Name,
            item.Country,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            $"{item.Pinned}",
            item.Checksum,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            Text.Truncate(item.Summary, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentAgreementView> ToAdjustmentAgreementViews(List<AdjustmentAgreement> items)
    {
        var views = new List<AdjustmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentAgreementView(item));
        }
        return views;
    }

    public static double SumAdjustmentAgreementView(AdjustmentAgreementView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCreatedAt + (double)parsedRetries;
    }
}
