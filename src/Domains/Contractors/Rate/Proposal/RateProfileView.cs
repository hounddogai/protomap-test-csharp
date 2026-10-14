namespace Acme.Workforce.Contractors.Rate.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate profile.</summary>
public sealed record RateProfileView(string Code, string Title, string Reviewer, string ChannelText, string ApprovedText, string Unit, string Description, string SourceText, string EmployeeId, string Score, string StatusLabel)
{
    public static RateProfileView ToRateProfileView(RateProfile item)
    {
        return new RateProfileView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            item.Reviewer,
            item.Channel,
            $"{item.Approved}",
            item.Unit,
            Text.Truncate(item.Description, 40),
            Text.Truncate(item.Source, 64),
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateProfileView> ToRateProfileViews(List<RateProfile> items)
    {
        var views = new List<RateProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRateProfileView(item));
        }
        return views;
    }

    public static double SumRateProfileView(RateProfileView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
