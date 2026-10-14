namespace Acme.Workforce.Absence.Accrual.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary accrual request.</summary>
public sealed record PrimaryAccrualRequestView(string CodeText, string TitleText, string Channel, string NotesText, string AmountText, string Archived, string PriorityText, string OwnerIdText, string JobCode, string StatusLabel)
{
    public static PrimaryAccrualRequestView ToPrimaryAccrualRequestView(PrimaryAccrualRequest item)
    {
        return new PrimaryAccrualRequestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Channel,
            item.Notes,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            Text.Truncate(item.JobCode, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAccrualRequestView> ToPrimaryAccrualRequestViews(List<PrimaryAccrualRequest> items)
    {
        var views = new List<PrimaryAccrualRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAccrualRequestView(item));
        }
        return views;
    }

    public static double SumPrimaryAccrualRequestView(PrimaryAccrualRequestView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedPriority;
    }
}
