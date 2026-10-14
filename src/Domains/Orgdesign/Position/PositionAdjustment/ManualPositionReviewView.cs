namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual position review.</summary>
public sealed record ManualPositionReviewView(string Code, string Name, string GradeText, string Ceiling, string Timezone, string Unit, string Region, string StatusLabel)
{
    public static ManualPositionReviewView ToManualPositionReviewView(ManualPositionReview item)
    {
        return new ManualPositionReviewView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Grade, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Unit,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPositionReviewView> ToManualPositionReviewViews(List<ManualPositionReview> items)
    {
        var views = new List<ManualPositionReviewView>();
        foreach (var item in items)
        {
            views.Add(ToManualPositionReviewView(item));
        }
        return views;
    }

    public static double SumManualPositionReviewView(ManualPositionReviewView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
