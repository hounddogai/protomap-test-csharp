namespace Acme.Workforce.Workplace.Visitor.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending visitor profile.</summary>
public sealed record PendingVisitorProfileView(string CodeText, string Title, string Amount, string SalaryBandText, string Skill, string LeaveDays, string Phone, string VisibleText, string Priority, string StatusLabel)
{
    public static PendingVisitorProfileView ToPendingVisitorProfileView(PendingVisitorProfile item)
    {
        return new PendingVisitorProfileView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Skill,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Visible}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVisitorProfileView> ToPendingVisitorProfileViews(List<PendingVisitorProfile> items)
    {
        var views = new List<PendingVisitorProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVisitorProfileView(item));
        }
        return views;
    }

    public static double SumPendingVisitorProfileView(PendingVisitorProfileView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedLeaveDays + (double)parsedPriority;
    }
}
