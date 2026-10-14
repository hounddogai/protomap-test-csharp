namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive career path slot.</summary>
public sealed record AdaptiveCareerPathSlotView(string CodeText, string Title, string Name, string Amount, string Version, string JobCode, string LocaleText, string Skill, string AttemptsText, string Timezone, string Description, string StatusLabel)
{
    public static AdaptiveCareerPathSlotView ToAdaptiveCareerPathSlotView(AdaptiveCareerPathSlot item)
    {
        return new AdaptiveCareerPathSlotView(
            item.Code,
            item.Title,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Locale,
            item.Skill,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveCareerPathSlotView> ToAdaptiveCareerPathSlotViews(List<AdaptiveCareerPathSlot> items)
    {
        var views = new List<AdaptiveCareerPathSlotView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveCareerPathSlotView(item));
        }
        return views;
    }

    public static double SumAdaptiveCareerPathSlotView(AdaptiveCareerPathSlotView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedVersion + (double)parsedAttempts;
    }
}
