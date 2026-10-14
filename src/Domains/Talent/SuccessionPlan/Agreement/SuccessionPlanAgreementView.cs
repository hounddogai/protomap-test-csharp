namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan agreement.</summary>
public sealed record SuccessionPlanAgreementView(string CodeText, string NameText, string GraceDaysText, string Country, string Revision, string Ratio, string Hours, string WindowDaysText, string Notes, string Discount, string StatusLabel)
{
    public static SuccessionPlanAgreementView ToSuccessionPlanAgreementView(SuccessionPlanAgreement item)
    {
        return new SuccessionPlanAgreementView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanAgreementView> ToSuccessionPlanAgreementViews(List<SuccessionPlanAgreement> items)
    {
        var views = new List<SuccessionPlanAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanAgreementView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanAgreementView(SuccessionPlanAgreementView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedRevision + parsedRatio;
    }
}
