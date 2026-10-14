namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due request.</summary>
public sealed record TrainingDueRequestView(string Code, string TitleText, string Retries, string MaximumText, string OwnerId, string Discount, string Fte, string WindowDaysText, string VisibleText, string Hours, string Name, string StatusLabel)
{
    public static TrainingDueRequestView ToTrainingDueRequestView(TrainingDueRequest item)
    {
        return new TrainingDueRequestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueRequestView> ToTrainingDueRequestViews(List<TrainingDueRequest> items)
    {
        var views = new List<TrainingDueRequestView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueRequestView(item));
        }
        return views;
    }

    public static double SumTrainingDueRequestView(TrainingDueRequestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMaximum + parsedDiscount;
    }
}
