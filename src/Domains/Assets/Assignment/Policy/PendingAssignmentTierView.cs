namespace Acme.Workforce.Assets.Assignment.Policy;

using System.Globalization;

/// <summary>How screens and exports show a pending assignment tier.</summary>
public sealed record PendingAssignmentTierView(string Code, string Title, string Retries, string OwnerId, string Currency, string ManagerIdText, string StatusLabel)
{
    public static PendingAssignmentTierView ToPendingAssignmentTierView(PendingAssignmentTier item)
    {
        return new PendingAssignmentTierView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Currency,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAssignmentTierView> ToPendingAssignmentTierViews(List<PendingAssignmentTier> items)
    {
        var views = new List<PendingAssignmentTierView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAssignmentTierView(item));
        }
        return views;
    }

    public static double SumPendingAssignmentTierView(PendingAssignmentTierView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
