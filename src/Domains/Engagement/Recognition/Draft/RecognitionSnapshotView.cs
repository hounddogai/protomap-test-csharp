namespace Acme.Workforce.Engagement.Recognition.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition snapshot.</summary>
public sealed record RecognitionSnapshotView(string Code, string Label, string SalaryBandText, string Sequence, string Source, string NameText, string StatusLabel)
{
    public static RecognitionSnapshotView ToRecognitionSnapshotView(RecognitionSnapshot item)
    {
        return new RecognitionSnapshotView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.SalaryBand,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionSnapshotView> ToRecognitionSnapshotViews(List<RecognitionSnapshot> items)
    {
        var views = new List<RecognitionSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionSnapshotView(item));
        }
        return views;
    }

    public static double SumRecognitionSnapshotView(RecognitionSnapshotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
