namespace Acme.Workforce.Absence.Policy.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient policy rollup.</summary>
public sealed record LenientPolicyRollupView(string Code, string Name, string ContactEmailText, string Label, string Minimum, string Department, string Revision, string Reviewer, string Locale, string StatusLabel)
{
    public static LenientPolicyRollupView ToLenientPolicyRollupView(LenientPolicyRollup item)
    {
        return new LenientPolicyRollupView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.ContactEmail,
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPolicyRollupView> ToLenientPolicyRollupViews(List<LenientPolicyRollup> items)
    {
        var views = new List<LenientPolicyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPolicyRollupView(item));
        }
        return views;
    }

    public static double SumLenientPolicyRollupView(LenientPolicyRollupView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedRevision;
    }
}
