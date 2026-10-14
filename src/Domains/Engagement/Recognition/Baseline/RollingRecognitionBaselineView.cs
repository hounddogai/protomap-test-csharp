namespace Acme.Workforce.Engagement.Recognition.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling recognition baseline.</summary>
public sealed record RollingRecognitionBaselineView(string Code, string TitleText, string Visible, string Minimum, string Skill, string Priority, string Sequence, string DepartmentText, string TenureMonths, string Score, string StatusLabel)
{
    public static RollingRecognitionBaselineView ToRollingRecognitionBaselineView(RollingRecognitionBaseline item)
    {
        return new RollingRecognitionBaselineView(
            Text.Truncate(item.Code, 40),
            item.Title,
            $"{item.Visible}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRecognitionBaselineView> ToRollingRecognitionBaselineViews(List<RollingRecognitionBaseline> items)
    {
        var views = new List<RollingRecognitionBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRecognitionBaselineView(item));
        }
        return views;
    }

    public static double SumRollingRecognitionBaselineView(RollingRecognitionBaselineView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedPriority + (double)parsedSequence;
    }
}
