namespace Acme.Workforce.Compensation.PayBand.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band milestone.</summary>
public sealed record PayBandMilestoneView(string Code, string Title, string MarginText, string Revision, string StatusLabel)
{
    public static PayBandMilestoneView ToPayBandMilestoneView(PayBandMilestone item)
    {
        return new PayBandMilestoneView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandMilestoneView> ToPayBandMilestoneViews(List<PayBandMilestone> items)
    {
        var views = new List<PayBandMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandMilestoneView(item));
        }
        return views;
    }

    public static double SumPayBandMilestoneView(PayBandMilestoneView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRevision;
    }
}
