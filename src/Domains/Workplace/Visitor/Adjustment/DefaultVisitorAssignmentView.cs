namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default visitor assignment.</summary>
public sealed record DefaultVisitorAssignmentView(string Code, string LabelText, string Source, string TenureMonthsText, string Description, string Active, string StatusLabel)
{
    public static DefaultVisitorAssignmentView ToDefaultVisitorAssignmentView(DefaultVisitorAssignment item)
    {
        return new DefaultVisitorAssignmentView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Source,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVisitorAssignmentView> ToDefaultVisitorAssignmentViews(List<DefaultVisitorAssignment> items)
    {
        var views = new List<DefaultVisitorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVisitorAssignmentView(item));
        }
        return views;
    }

    public static double SumDefaultVisitorAssignmentView(DefaultVisitorAssignmentView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
