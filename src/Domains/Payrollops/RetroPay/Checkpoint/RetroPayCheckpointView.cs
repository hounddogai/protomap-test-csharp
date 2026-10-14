namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay checkpoint.</summary>
public sealed record RetroPayCheckpointView(string Code, string Label, string DepartmentText, string EmployeeId, string Revision, string VersionText, string TenureMonthsText, string Score, string AttemptsText, string StartsAtText, string BatchSizeText, string StatusLabel)
{
    public static RetroPayCheckpointView ToRetroPayCheckpointView(RetroPayCheckpoint item)
    {
        return new RetroPayCheckpointView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Department,
            item.EmployeeId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayCheckpointView> ToRetroPayCheckpointViews(List<RetroPayCheckpoint> items)
    {
        var views = new List<RetroPayCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayCheckpointView(item));
        }
        return views;
    }

    public static double SumRetroPayCheckpointView(RetroPayCheckpointView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedVersion + (double)parsedTenureMonths;
    }
}
