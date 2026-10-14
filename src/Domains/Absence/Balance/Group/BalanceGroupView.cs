namespace Acme.Workforce.Absence.Balance.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance group.</summary>
public sealed record BalanceGroupView(string Code, string LabelText, string EmployeeId, string Notes, string SequenceText, string Locale, string Reviewer, string PinnedText, string Location, string Phone, string StatusLabel)
{
    public static BalanceGroupView ToBalanceGroupView(BalanceGroup item)
    {
        return new BalanceGroupView(
            item.Code,
            item.Label,
            item.EmployeeId,
            Text.Truncate(item.Notes, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Reviewer, 80),
            $"{item.Pinned}",
            Text.Truncate(item.Location, 64),
            Text.Truncate(item.Phone, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceGroupView> ToBalanceGroupViews(List<BalanceGroup> items)
    {
        var views = new List<BalanceGroupView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceGroupView(item));
        }
        return views;
    }

    public static double SumBalanceGroupView(BalanceGroupView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
