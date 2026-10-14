namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay adjustment.</summary>
public sealed record RetroPayAdjustmentView(string Code, string Label, string LeaveDaysText, string Revision, string ReviewerText, string CostCenterText, string StatusLabel)
{
    public static RetroPayAdjustmentView ToRetroPayAdjustmentView(RetroPayAdjustment item)
    {
        return new RetroPayAdjustmentView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayAdjustmentView> ToRetroPayAdjustmentViews(List<RetroPayAdjustment> items)
    {
        var views = new List<RetroPayAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayAdjustmentView(item));
        }
        return views;
    }

    public static double SumRetroPayAdjustmentView(RetroPayAdjustmentView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedRevision;
    }
}
