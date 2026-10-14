namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped equity grant score.</summary>
public sealed record CappedEquityGrantScoreView(string Code, string NameText, string AttemptsText, string RevisionText, string JobCode, string CountryText, string StatusLabel)
{
    public static CappedEquityGrantScoreView ToCappedEquityGrantScoreView(CappedEquityGrantScore item)
    {
        return new CappedEquityGrantScoreView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.Country, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedEquityGrantScoreView> ToCappedEquityGrantScoreViews(List<CappedEquityGrantScore> items)
    {
        var views = new List<CappedEquityGrantScoreView>();
        foreach (var item in items)
        {
            views.Add(ToCappedEquityGrantScoreView(item));
        }
        return views;
    }

    public static double SumCappedEquityGrantScoreView(CappedEquityGrantScoreView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedRevision;
    }
}
