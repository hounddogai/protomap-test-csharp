namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant review.</summary>
public sealed record EquityGrantReviewView(string Code, string Name, string Budget, string Minimum, string Reviewer, string Timezone, string Ceiling, string BatchSize, string Hours, string Active, string StatusLabel)
{
    public static EquityGrantReviewView ToEquityGrantReviewView(EquityGrantReview item)
    {
        return new EquityGrantReviewView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.Timezone,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantReviewView> ToEquityGrantReviewViews(List<EquityGrantReview> items)
    {
        var views = new List<EquityGrantReviewView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantReviewView(item));
        }
        return views;
    }

    public static double SumEquityGrantReviewView(EquityGrantReviewView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMinimum + parsedCeiling;
    }
}
