namespace Acme.Workforce.Absence.Accrual.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual checkpoint.</summary>
public sealed record AccrualCheckpointView(string CodeText, string Title, string Minimum, string DescriptionText, string StatusLabel)
{
    public static AccrualCheckpointView ToAccrualCheckpointView(AccrualCheckpoint item)
    {
        return new AccrualCheckpointView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualCheckpointView> ToAccrualCheckpointViews(List<AccrualCheckpoint> items)
    {
        var views = new List<AccrualCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualCheckpointView(item));
        }
        return views;
    }

    public static double SumAccrualCheckpointView(AccrualCheckpointView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
