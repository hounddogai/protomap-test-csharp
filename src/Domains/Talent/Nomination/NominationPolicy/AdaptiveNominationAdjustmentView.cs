namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive nomination adjustment.</summary>
public sealed record AdaptiveNominationAdjustmentView(string Code, string LabelText, string Hours, string PhoneText, string Name, string EndsAt, string TenureMonthsText, string StatusLabel)
{
    public static AdaptiveNominationAdjustmentView ToAdaptiveNominationAdjustmentView(AdaptiveNominationAdjustment item)
    {
        return new AdaptiveNominationAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveNominationAdjustmentView> ToAdaptiveNominationAdjustmentViews(List<AdaptiveNominationAdjustment> items)
    {
        var views = new List<AdaptiveNominationAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveNominationAdjustmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveNominationAdjustmentView(AdaptiveNominationAdjustmentView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
