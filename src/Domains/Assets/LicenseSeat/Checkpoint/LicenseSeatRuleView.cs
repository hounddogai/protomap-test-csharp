namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat rule.</summary>
public sealed record LicenseSeatRuleView(string CodeText, string TitleText, string CostCenter, string OwnerId, string Pinned, string Phone, string AmountText, string MinimumText, string StatusLabel)
{
    public static LicenseSeatRuleView ToLicenseSeatRuleView(LicenseSeatRule item)
    {
        return new LicenseSeatRuleView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.CostCenter,
            item.OwnerId,
            $"{item.Pinned}",
            item.Phone,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatRuleView> ToLicenseSeatRuleViews(List<LicenseSeatRule> items)
    {
        var views = new List<LicenseSeatRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatRuleView(item));
        }
        return views;
    }

    public static double SumLicenseSeatRuleView(LicenseSeatRuleView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMinimum;
    }
}
