namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default adjustment agreement.</summary>
public sealed record DefaultAdjustmentAgreementView(string Code, string TitleText, string Team, string CapacityText, string CostCenterText, string TimezoneText, string Priority, string Country, string Minimum, string StatusLabel)
{
    public static DefaultAdjustmentAgreementView ToDefaultAdjustmentAgreementView(DefaultAdjustmentAgreement item)
    {
        return new DefaultAdjustmentAgreementView(
            item.Code,
            item.Title,
            item.Team,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Timezone, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAdjustmentAgreementView> ToDefaultAdjustmentAgreementViews(List<DefaultAdjustmentAgreement> items)
    {
        var views = new List<DefaultAdjustmentAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAdjustmentAgreementView(item));
        }
        return views;
    }

    public static double SumDefaultAdjustmentAgreementView(DefaultAdjustmentAgreementView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedPriority + parsedMinimum;
    }
}
