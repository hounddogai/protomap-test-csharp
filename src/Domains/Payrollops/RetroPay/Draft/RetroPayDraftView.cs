namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay draft.</summary>
public sealed record RetroPayDraftView(string Code, string Title, string Phone, string Name, string HoursText, string SalaryBand, string Weight, string PriorityText, string Retries, string StatusLabel)
{
    public static RetroPayDraftView ToRetroPayDraftView(RetroPayDraft item)
    {
        return new RetroPayDraftView(
            item.Code,
            item.Title,
            item.Phone,
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayDraftView> ToRetroPayDraftViews(List<RetroPayDraft> items)
    {
        var views = new List<RetroPayDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayDraftView(item));
        }
        return views;
    }

    public static double SumRetroPayDraftView(RetroPayDraftView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedHours + parsedWeight + (double)parsedPriority;
    }
}
