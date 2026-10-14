namespace Acme.Workforce.Absence.Policy.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional policy assignment.</summary>
public sealed record RegionalPolicyAssignmentView(string Code, string Title, string Ratio, string Description, string UnitText, string ContactEmail, string StatusLabel)
{
    public static RegionalPolicyAssignmentView ToRegionalPolicyAssignmentView(RegionalPolicyAssignment item)
    {
        return new RegionalPolicyAssignmentView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Unit, 120),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPolicyAssignmentView> ToRegionalPolicyAssignmentViews(List<RegionalPolicyAssignment> items)
    {
        var views = new List<RegionalPolicyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPolicyAssignmentView(item));
        }
        return views;
    }

    public static double SumRegionalPolicyAssignmentView(RegionalPolicyAssignmentView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
