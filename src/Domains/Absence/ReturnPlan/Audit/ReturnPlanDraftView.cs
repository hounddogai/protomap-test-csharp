namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan draft.</summary>
public sealed record ReturnPlanDraftView(string Code, string LabelText, string CurrencyText, string ContactEmail, string Archived, string DescriptionText, string VisibleText, string Hours, string Approved, string StatusLabel)
{
    public static ReturnPlanDraftView ToReturnPlanDraftView(ReturnPlanDraft item)
    {
        return new ReturnPlanDraftView(
            item.Code,
            item.Label,
            Text.Truncate(item.Currency, 64),
            Text.Truncate(item.ContactEmail, 40),
            $"{item.Archived}",
            item.Description,
            $"{item.Visible}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanDraftView> ToReturnPlanDraftViews(List<ReturnPlanDraft> items)
    {
        var views = new List<ReturnPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanDraftView(item));
        }
        return views;
    }

    public static double SumReturnPlanDraftView(ReturnPlanDraftView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
