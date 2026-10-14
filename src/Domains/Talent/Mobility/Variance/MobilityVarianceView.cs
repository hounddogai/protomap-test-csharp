namespace Acme.Workforce.Talent.Mobility.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility variance.</summary>
public sealed record MobilityVarianceView(string Code, string Title, string Limit, string ManagerId, string Rate, string Team, string StatusLabel)
{
    public static MobilityVarianceView ToMobilityVarianceView(MobilityVariance item)
    {
        return new MobilityVarianceView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityVarianceView> ToMobilityVarianceViews(List<MobilityVariance> items)
    {
        var views = new List<MobilityVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityVarianceView(item));
        }
        return views;
    }

    public static double SumMobilityVarianceView(MobilityVarianceView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRate;
    }
}
