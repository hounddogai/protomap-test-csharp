namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding baseline.</summary>
public sealed record OffboardingBaselineView(string Code, string Title, string Approved, string CreatedAt, string Limit, string Sequence, string Rate, string LabelText, string Minimum, string StatusLabel)
{
    public static OffboardingBaselineView ToOffboardingBaselineView(OffboardingBaseline item)
    {
        return new OffboardingBaselineView(
            item.Code,
            Text.Truncate(item.Title, 40),
            $"{item.Approved}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingBaselineView> ToOffboardingBaselineViews(List<OffboardingBaseline> items)
    {
        var views = new List<OffboardingBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingBaselineView(item));
        }
        return views;
    }

    public static double SumOffboardingBaselineView(OffboardingBaselineView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLimit + (double)parsedSequence;
    }
}
