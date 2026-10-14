namespace Acme.Workforce.Payrollops.TaxForm.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled tax form template.</summary>
public sealed record ScheduledTaxFormTemplateView(string Code, string NameText, string ExternalRefText, string EndsAtText, string Skill, string ReviewerText, string Badge, string Active, string GradeText, string TenureMonths, string Region, string StatusLabel)
{
    public static ScheduledTaxFormTemplateView ToScheduledTaxFormTemplateView(ScheduledTaxFormTemplate item)
    {
        return new ScheduledTaxFormTemplateView(
            item.Code,
            item.Name,
            Text.Truncate(item.ExternalRef, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Reviewer, 80),
            item.Badge,
            $"{item.Active}",
            item.Grade,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTaxFormTemplateView> ToScheduledTaxFormTemplateViews(List<ScheduledTaxFormTemplate> items)
    {
        var views = new List<ScheduledTaxFormTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTaxFormTemplateView(item));
        }
        return views;
    }

    public static double SumScheduledTaxFormTemplateView(ScheduledTaxFormTemplateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
