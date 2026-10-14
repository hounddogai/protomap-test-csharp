namespace Acme.Workforce.Workplace.Visitor.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling visitor window.</summary>
public sealed record RollingVisitorWindowView(string Code, string NameText, string RevisionText, string Score, string Archived, string Minimum, string Rate, string ThresholdText, string FteText, string Channel, string StatusLabel)
{
    public static RollingVisitorWindowView ToRollingVisitorWindowView(RollingVisitorWindow item)
    {
        return new RollingVisitorWindowView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingVisitorWindowView> ToRollingVisitorWindowViews(List<RollingVisitorWindow> items)
    {
        var views = new List<RollingVisitorWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRollingVisitorWindowView(item));
        }
        return views;
    }

    public static double SumRollingVisitorWindowView(RollingVisitorWindowView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedScore + parsedMinimum;
    }
}
