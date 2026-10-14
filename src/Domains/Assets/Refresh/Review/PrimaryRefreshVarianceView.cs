namespace Acme.Workforce.Assets.Refresh.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary refresh variance.</summary>
public sealed record PrimaryRefreshVarianceView(string Code, string LabelText, string PhoneText, string GraceDays, string GradeText, string Title, string StartsAt, string Cost, string Revision, string Budget, string Weight, string StatusLabel)
{
    public static PrimaryRefreshVarianceView ToPrimaryRefreshVarianceView(PrimaryRefreshVariance item)
    {
        return new PrimaryRefreshVarianceView(
            item.Code,
            item.Label,
            item.Phone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRefreshVarianceView> ToPrimaryRefreshVarianceViews(List<PrimaryRefreshVariance> items)
    {
        var views = new List<PrimaryRefreshVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRefreshVarianceView(item));
        }
        return views;
    }

    public static double SumPrimaryRefreshVarianceView(PrimaryRefreshVarianceView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedStartsAt + parsedCost;
    }
}
