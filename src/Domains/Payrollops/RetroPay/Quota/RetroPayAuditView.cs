namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay audit.</summary>
public sealed record RetroPayAuditView(string Code, string Name, string Source, string StartsAt, string Margin, string StatusLabel)
{
    public static RetroPayAuditView ToRetroPayAuditView(RetroPayAudit item)
    {
        return new RetroPayAuditView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Source,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayAuditView> ToRetroPayAuditViews(List<RetroPayAudit> items)
    {
        var views = new List<RetroPayAuditView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayAuditView(item));
        }
        return views;
    }

    public static double SumRetroPayAuditView(RetroPayAuditView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMargin;
    }
}
