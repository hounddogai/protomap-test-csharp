namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using System.Globalization;

/// <summary>How screens and exports show a rolling training due exception.</summary>
public sealed record RollingTrainingDueExceptionView(string Code, string NameText, string Sequence, string OwnerId, string CreatedAt, string Timezone, string Margin, string StatusLabel)
{
    public static RollingTrainingDueExceptionView ToRollingTrainingDueExceptionView(RollingTrainingDueException item)
    {
        return new RollingTrainingDueExceptionView(
            item.Code,
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingTrainingDueExceptionView> ToRollingTrainingDueExceptionViews(List<RollingTrainingDueException> items)
    {
        var views = new List<RollingTrainingDueExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRollingTrainingDueExceptionView(item));
        }
        return views;
    }

    public static double SumRollingTrainingDueExceptionView(RollingTrainingDueExceptionView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedCreatedAt + parsedMargin;
    }
}
