namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual training due notice.</summary>
public sealed record ManualTrainingDueNoticeView(string CodeText, string Name, string Capacity, string Margin, string EmployeeIdText, string StatusLabel)
{
    public static ManualTrainingDueNoticeView ToManualTrainingDueNoticeView(ManualTrainingDueNotice item)
    {
        return new ManualTrainingDueNoticeView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualTrainingDueNoticeView> ToManualTrainingDueNoticeViews(List<ManualTrainingDueNotice> items)
    {
        var views = new List<ManualTrainingDueNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToManualTrainingDueNoticeView(item));
        }
        return views;
    }

    public static double SumManualTrainingDueNoticeView(ManualTrainingDueNoticeView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMargin;
    }
}
