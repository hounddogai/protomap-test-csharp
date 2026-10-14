namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary equity grant override.</summary>
public sealed record PrimaryEquityGrantOverrideView(string Code, string Title, string Team, string Checksum, string ContactEmailText, string Active, string ScoreText, string StatusLabel)
{
    public static PrimaryEquityGrantOverrideView ToPrimaryEquityGrantOverrideView(PrimaryEquityGrantOverride item)
    {
        return new PrimaryEquityGrantOverrideView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Team,
            item.Checksum,
            Text.Truncate(item.ContactEmail, 64),
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryEquityGrantOverrideView> ToPrimaryEquityGrantOverrideViews(List<PrimaryEquityGrantOverride> items)
    {
        var views = new List<PrimaryEquityGrantOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryEquityGrantOverrideView(item));
        }
        return views;
    }

    public static double SumPrimaryEquityGrantOverrideView(PrimaryEquityGrantOverrideView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
