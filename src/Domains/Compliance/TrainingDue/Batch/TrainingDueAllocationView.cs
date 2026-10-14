namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due allocation.</summary>
public sealed record TrainingDueAllocationView(string Code, string Title, string NotesText, string Visible, string Checksum, string Limit, string Archived, string FteText, string Location, string StatusLabel)
{
    public static TrainingDueAllocationView ToTrainingDueAllocationView(TrainingDueAllocation item)
    {
        return new TrainingDueAllocationView(
            item.Code,
            item.Title,
            item.Notes,
            $"{item.Visible}",
            item.Checksum,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueAllocationView> ToTrainingDueAllocationViews(List<TrainingDueAllocation> items)
    {
        var views = new List<TrainingDueAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueAllocationView(item));
        }
        return views;
    }

    public static double SumTrainingDueAllocationView(TrainingDueAllocationView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedFte;
    }
}
