namespace Acme.Workforce.Payrollops.Deduction.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional deduction batch.</summary>
public sealed record RegionalDeductionBatchView(string Code, string NameText, string Maximum, string EndsAt, string TenureMonths, string Source, string Cost, string SkillText, string OwnerId, string Archived, string VersionText, string StatusLabel)
{
    public static RegionalDeductionBatchView ToRegionalDeductionBatchView(RegionalDeductionBatch item)
    {
        return new RegionalDeductionBatchView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.OwnerId, 80),
            $"{item.Archived}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeductionBatchView> ToRegionalDeductionBatchViews(List<RegionalDeductionBatch> items)
    {
        var views = new List<RegionalDeductionBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeductionBatchView(item));
        }
        return views;
    }

    public static double SumRegionalDeductionBatchView(RegionalDeductionBatchView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
