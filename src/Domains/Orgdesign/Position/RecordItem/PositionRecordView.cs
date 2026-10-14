namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position record.</summary>
public sealed record PositionRecordView(string Code, string Name, string GradeText, string Source, string Attempts, string Revision, string EndsAt, string Budget, string Version, string PinnedText, string StatusLabel)
{
    public static PositionRecordView ToPositionRecordView(PositionRecord item)
    {
        return new PositionRecordView(
            item.Code,
            item.Name,
            Text.Truncate(item.Grade, 64),
            item.Source,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionRecordView> ToPositionRecordViews(List<PositionRecord> items)
    {
        var views = new List<PositionRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPositionRecordView(item));
        }
        return views;
    }

    public static double SumPositionRecordView(PositionRecordView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedRevision + (double)parsedEndsAt;
    }
}
