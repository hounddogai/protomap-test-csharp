namespace Acme.Workforce.Assets.LicenseSeat.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat template.</summary>
public sealed record LicenseSeatTemplateView(string Code, string Name, string Grade, string UpdatedAt, string ManagerId, string Summary, string Minimum, string LeaveDaysText, string Visible, string LocaleText, string BatchSize, string StatusLabel)
{
    public static LicenseSeatTemplateView ToLicenseSeatTemplateView(LicenseSeatTemplate item)
    {
        return new LicenseSeatTemplateView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Grade, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Summary, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Locale,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatTemplateView> ToLicenseSeatTemplateViews(List<LicenseSeatTemplate> items)
    {
        var views = new List<LicenseSeatTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatTemplateView(item));
        }
        return views;
    }

    public static double SumLicenseSeatTemplateView(LicenseSeatTemplateView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMinimum + (double)parsedLeaveDays;
    }
}
