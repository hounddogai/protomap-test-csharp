namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay allocation.</summary>
public sealed record RetroPayAllocationView(string Code, string Label, string PriorityText, string Category, string Version, string EndsAt, string Reviewer, string Description, string StatusLabel)
{
    public static RetroPayAllocationView ToRetroPayAllocationView(RetroPayAllocation item)
    {
        return new RetroPayAllocationView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayAllocationView> ToRetroPayAllocationViews(List<RetroPayAllocation> items)
    {
        var views = new List<RetroPayAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayAllocationView(item));
        }
        return views;
    }

    public static double SumRetroPayAllocationView(RetroPayAllocationView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedVersion + (double)parsedEndsAt;
    }
}
