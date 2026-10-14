namespace Acme.Workforce.Talent.Nomination.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict nomination tier.</summary>
public sealed record StrictNominationTierView(string Code, string Title, string TimezoneText, string EmployeeIdText, string Badge, string BatchSize, string StatusLabel)
{
    public static StrictNominationTierView ToStrictNominationTierView(StrictNominationTier item)
    {
        return new StrictNominationTierView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Timezone,
            item.EmployeeId,
            item.Badge,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictNominationTierView> ToStrictNominationTierViews(List<StrictNominationTier> items)
    {
        var views = new List<StrictNominationTierView>();
        foreach (var item in items)
        {
            views.Add(ToStrictNominationTierView(item));
        }
        return views;
    }

    public static double SumStrictNominationTierView(StrictNominationTierView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
