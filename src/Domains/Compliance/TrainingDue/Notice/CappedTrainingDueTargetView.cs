namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using System.Globalization;

/// <summary>How screens and exports show a capped training due target.</summary>
public sealed record CappedTrainingDueTargetView(string Code, string Label, string GraceDays, string ChecksumText, string LeaveDays, string StatusLabel)
{
    public static CappedTrainingDueTargetView ToCappedTrainingDueTargetView(CappedTrainingDueTarget item)
    {
        return new CappedTrainingDueTargetView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedTrainingDueTargetView> ToCappedTrainingDueTargetViews(List<CappedTrainingDueTarget> items)
    {
        var views = new List<CappedTrainingDueTargetView>();
        foreach (var item in items)
        {
            views.Add(ToCappedTrainingDueTargetView(item));
        }
        return views;
    }

    public static double SumCappedTrainingDueTargetView(CappedTrainingDueTargetView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedLeaveDays;
    }
}
