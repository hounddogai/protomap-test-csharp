namespace Acme.Workforce.Talent.Nomination.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default nomination assignment.</summary>
public sealed record DefaultNominationAssignmentView(string Code, string Title, string Phone, string Description, string SalaryBand, string Minimum, string Attempts, string ExternalRefText, string StatusLabel)
{
    public static DefaultNominationAssignmentView ToDefaultNominationAssignmentView(DefaultNominationAssignment item)
    {
        return new DefaultNominationAssignmentView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Phone,
            Text.Truncate(item.Description, 40),
            item.SalaryBand,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultNominationAssignmentView> ToDefaultNominationAssignmentViews(List<DefaultNominationAssignment> items)
    {
        var views = new List<DefaultNominationAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultNominationAssignmentView(item));
        }
        return views;
    }

    public static double SumDefaultNominationAssignmentView(DefaultNominationAssignmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedAttempts;
    }
}
