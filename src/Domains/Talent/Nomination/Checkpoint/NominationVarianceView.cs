namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination variance.</summary>
public sealed record NominationVarianceView(string CodeText, string Label, string ExpiresAtText, string Grade, string CapacityText, string ContactEmail, string LeaveDaysText, string StatusLabel)
{
    public static NominationVarianceView ToNominationVarianceView(NominationVariance item)
    {
        return new NominationVarianceView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationVarianceView> ToNominationVarianceViews(List<NominationVariance> items)
    {
        var views = new List<NominationVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToNominationVarianceView(item));
        }
        return views;
    }

    public static double SumNominationVarianceView(NominationVarianceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedCapacity + (double)parsedLeaveDays;
    }
}
