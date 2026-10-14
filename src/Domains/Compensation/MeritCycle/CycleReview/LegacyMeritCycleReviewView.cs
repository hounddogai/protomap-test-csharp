namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy merit cycle review.</summary>
public sealed record LegacyMeritCycleReviewView(string CodeText, string Name, string PinnedText, string OwnerIdText, string LeaveDays, string StatusLabel)
{
    public static LegacyMeritCycleReviewView ToLegacyMeritCycleReviewView(LegacyMeritCycleReview item)
    {
        return new LegacyMeritCycleReviewView(
            Text.Truncate(item.Code, 40),
            item.Name,
            $"{item.Pinned}",
            Text.Truncate(item.OwnerId, 80),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyMeritCycleReviewView> ToLegacyMeritCycleReviewViews(List<LegacyMeritCycleReview> items)
    {
        var views = new List<LegacyMeritCycleReviewView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyMeritCycleReviewView(item));
        }
        return views;
    }

    public static double SumLegacyMeritCycleReviewView(LegacyMeritCycleReviewView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
