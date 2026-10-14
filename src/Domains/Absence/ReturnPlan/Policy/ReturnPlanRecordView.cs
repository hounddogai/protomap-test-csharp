namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan record.</summary>
public sealed record ReturnPlanRecordView(string Code, string LabelText, string AttemptsText, string GraceDaysText, string WindowDays, string StatusLabel)
{
    public static ReturnPlanRecordView ToReturnPlanRecordView(ReturnPlanRecord item)
    {
        return new ReturnPlanRecordView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanRecordView> ToReturnPlanRecordViews(List<ReturnPlanRecord> items)
    {
        var views = new List<ReturnPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanRecordView(item));
        }
        return views;
    }

    public static double SumReturnPlanRecordView(ReturnPlanRecordView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedGraceDays + (double)parsedWindowDays;
    }
}
