namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback skill profile snapshot.</summary>
public sealed record FallbackSkillProfileSnapshotView(string Code, string Label, string Skill, string GraceDays, string Capacity, string Archived, string Visible, string Quantity, string StatusLabel)
{
    public static FallbackSkillProfileSnapshotView ToFallbackSkillProfileSnapshotView(FallbackSkillProfileSnapshot item)
    {
        return new FallbackSkillProfileSnapshotView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Skill,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Visible}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSkillProfileSnapshotView> ToFallbackSkillProfileSnapshotViews(List<FallbackSkillProfileSnapshot> items)
    {
        var views = new List<FallbackSkillProfileSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSkillProfileSnapshotView(item));
        }
        return views;
    }

    public static double SumFallbackSkillProfileSnapshotView(FallbackSkillProfileSnapshotView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedCapacity + (double)parsedQuantity;
    }
}
