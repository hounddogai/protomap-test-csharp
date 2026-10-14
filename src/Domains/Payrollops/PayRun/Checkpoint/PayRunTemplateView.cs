namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run template.</summary>
public sealed record PayRunTemplateView(string Code, string Label, string Ratio, string CreatedAt, string SummaryText, string ContactEmail, string MinimumText, string RevisionText, string Grade, string Ceiling, string Reviewer, string StatusLabel)
{
    public static PayRunTemplateView ToPayRunTemplateView(PayRunTemplate item)
    {
        return new PayRunTemplateView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            Text.Truncate(item.ContactEmail, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunTemplateView> ToPayRunTemplateViews(List<PayRunTemplate> items)
    {
        var views = new List<PayRunTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunTemplateView(item));
        }
        return views;
    }

    public static double SumPayRunTemplateView(PayRunTemplateView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedCreatedAt + parsedMinimum;
    }
}
