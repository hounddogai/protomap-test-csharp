namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan agreement.</summary>
public sealed record FloorPlanAgreementView(string CodeText, string Name, string Amount, string Channel, string Revision, string Score, string ContactEmail, string Label, string Locale, string UpdatedAt, string StatusLabel)
{
    public static FloorPlanAgreementView ToFloorPlanAgreementView(FloorPlanAgreement item)
    {
        return new FloorPlanAgreementView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Label,
            item.Locale,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanAgreementView> ToFloorPlanAgreementViews(List<FloorPlanAgreement> items)
    {
        var views = new List<FloorPlanAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanAgreementView(item));
        }
        return views;
    }

    public static double SumFloorPlanAgreementView(FloorPlanAgreementView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedRevision + parsedScore;
    }
}
