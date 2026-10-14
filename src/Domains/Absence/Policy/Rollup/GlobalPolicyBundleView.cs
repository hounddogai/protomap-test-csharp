namespace Acme.Workforce.Absence.Policy.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global policy bundle.</summary>
public sealed record GlobalPolicyBundleView(string CodeText, string LabelText, string StartsAtText, string Unit, string DescriptionText, string Visible, string Revision, string SequenceText, string StatusLabel)
{
    public static GlobalPolicyBundleView ToGlobalPolicyBundleView(GlobalPolicyBundle item)
    {
        return new GlobalPolicyBundleView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Description,
            $"{item.Visible}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPolicyBundleView> ToGlobalPolicyBundleViews(List<GlobalPolicyBundle> items)
    {
        var views = new List<GlobalPolicyBundleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPolicyBundleView(item));
        }
        return views;
    }

    public static double SumGlobalPolicyBundleView(GlobalPolicyBundleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedRevision + (double)parsedSequence;
    }
}
