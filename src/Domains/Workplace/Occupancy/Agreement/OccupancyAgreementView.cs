namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy agreement.</summary>
public sealed record OccupancyAgreementView(string Code, string TitleText, string CostCenter, string EndsAt, string Team, string LimitText, string StatusLabel)
{
    public static OccupancyAgreementView ToOccupancyAgreementView(OccupancyAgreement item)
    {
        return new OccupancyAgreementView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.CostCenter,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyAgreementView> ToOccupancyAgreementViews(List<OccupancyAgreement> items)
    {
        var views = new List<OccupancyAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyAgreementView(item));
        }
        return views;
    }

    public static double SumOccupancyAgreementView(OccupancyAgreementView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedLimit;
    }
}
