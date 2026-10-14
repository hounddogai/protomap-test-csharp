namespace Acme.Workforce.Talent.Nomination.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination policy.</summary>
public sealed record NominationPolicyView(string Code, string Label, string JobCode, string Minimum, string CapacityText, string StartsAt, string Grade, string StatusLabel)
{
    public static NominationPolicyView ToNominationPolicyView(NominationPolicy item)
    {
        return new NominationPolicyView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.JobCode,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationPolicyView> ToNominationPolicyViews(List<NominationPolicy> items)
    {
        var views = new List<NominationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToNominationPolicyView(item));
        }
        return views;
    }

    public static double SumNominationPolicyView(NominationPolicyView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCapacity + (double)parsedStartsAt;
    }
}
