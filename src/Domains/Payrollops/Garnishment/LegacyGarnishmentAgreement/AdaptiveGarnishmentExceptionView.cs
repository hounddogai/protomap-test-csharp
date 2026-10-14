namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive garnishment exception.</summary>
public sealed record AdaptiveGarnishmentExceptionView(string Code, string Name, string Department, string WindowDays, string EndsAt, string Skill, string BadgeText, string BatchSize, string StatusLabel)
{
    public static AdaptiveGarnishmentExceptionView ToAdaptiveGarnishmentExceptionView(AdaptiveGarnishmentException item)
    {
        return new AdaptiveGarnishmentExceptionView(
            Text.Truncate(item.Code, 40),
            item.Name,
            Text.Truncate(item.Department, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Badge,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveGarnishmentExceptionView> ToAdaptiveGarnishmentExceptionViews(List<AdaptiveGarnishmentException> items)
    {
        var views = new List<AdaptiveGarnishmentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveGarnishmentExceptionView(item));
        }
        return views;
    }

    public static double SumAdaptiveGarnishmentExceptionView(AdaptiveGarnishmentExceptionView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
