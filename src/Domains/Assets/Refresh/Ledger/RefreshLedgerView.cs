namespace Acme.Workforce.Assets.Refresh.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh ledger.</summary>
public sealed record RefreshLedgerView(string CodeText, string LabelText, string Discount, string Archived, string Category, string WindowDays, string Unit, string Country, string Reviewer, string SkillText, string FteText, string StatusLabel)
{
    public static RefreshLedgerView ToRefreshLedgerView(RefreshLedger item)
    {
        return new RefreshLedgerView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Category,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Country,
            Text.Truncate(item.Reviewer, 120),
            item.Skill,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshLedgerView> ToRefreshLedgerViews(List<RefreshLedger> items)
    {
        var views = new List<RefreshLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshLedgerView(item));
        }
        return views;
    }

    public static double SumRefreshLedgerView(RefreshLedgerView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedWindowDays + parsedFte;
    }
}
