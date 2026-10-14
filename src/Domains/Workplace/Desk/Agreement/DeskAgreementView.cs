namespace Acme.Workforce.Workplace.Desk.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk agreement.</summary>
public sealed record DeskAgreementView(string Code, string Label, string Ratio, string Badge, string JobCodeText, string Grade, string BatchSizeText, string Pinned, string Hours, string DescriptionText, string StatusLabel)
{
    public static DeskAgreementView ToDeskAgreementView(DeskAgreement item)
    {
        return new DeskAgreementView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.JobCode,
            Text.Truncate(item.Grade, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskAgreementView> ToDeskAgreementViews(List<DeskAgreement> items)
    {
        var views = new List<DeskAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToDeskAgreementView(item));
        }
        return views;
    }

    public static double SumDeskAgreementView(DeskAgreementView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedBatchSize + parsedHours;
    }
}
