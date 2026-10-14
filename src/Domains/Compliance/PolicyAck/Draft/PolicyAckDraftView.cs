namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack draft.</summary>
public sealed record PolicyAckDraftView(string Code, string TitleText, string DurationMinutesText, string NotesText, string Version, string Discount, string Timezone, string StatusLabel)
{
    public static PolicyAckDraftView ToPolicyAckDraftView(PolicyAckDraft item)
    {
        return new PolicyAckDraftView(
            item.Code,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckDraftView> ToPolicyAckDraftViews(List<PolicyAckDraft> items)
    {
        var views = new List<PolicyAckDraftView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckDraftView(item));
        }
        return views;
    }

    public static double SumPolicyAckDraftView(PolicyAckDraftView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedVersion + parsedDiscount;
    }
}
