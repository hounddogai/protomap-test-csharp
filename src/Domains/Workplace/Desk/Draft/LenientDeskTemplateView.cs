namespace Acme.Workforce.Workplace.Desk.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient desk template.</summary>
public sealed record LenientDeskTemplateView(string Code, string Name, string StartsAt, string CostText, string JobCode, string Approved, string Channel, string LeaveDays, string Reviewer, string ContactEmail, string StatusLabel)
{
    public static LenientDeskTemplateView ToLenientDeskTemplateView(LenientDeskTemplate item)
    {
        return new LenientDeskTemplateView(
            item.Code,
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            $"{item.Approved}",
            item.Channel,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeskTemplateView> ToLenientDeskTemplateViews(List<LenientDeskTemplate> items)
    {
        var views = new List<LenientDeskTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeskTemplateView(item));
        }
        return views;
    }

    public static double SumLenientDeskTemplateView(LenientDeskTemplateView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedCost + (double)parsedLeaveDays;
    }
}
