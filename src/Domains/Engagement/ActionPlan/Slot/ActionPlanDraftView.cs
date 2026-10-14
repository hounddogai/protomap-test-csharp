namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using System.Globalization;

/// <summary>How screens and exports show a action plan draft.</summary>
public sealed record ActionPlanDraftView(string Code, string LabelText, string NotesText, string Phone, string WindowDaysText, string SalaryBand, string Limit, string UpdatedAtText, string StatusLabel)
{
    public static ActionPlanDraftView ToActionPlanDraftView(ActionPlanDraft item)
    {
        return new ActionPlanDraftView(
            item.Code,
            item.Label,
            item.Notes,
            item.Phone,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanDraftView> ToActionPlanDraftViews(List<ActionPlanDraft> items)
    {
        var views = new List<ActionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanDraftView(item));
        }
        return views;
    }

    public static double SumActionPlanDraftView(ActionPlanDraftView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedLimit + (double)parsedUpdatedAt;
    }
}
