namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive training due exception.</summary>
public sealed record AdaptiveTrainingDueExceptionView(string Code, string Name, string CreatedAt, string CostText, string Maximum, string Archived, string ContactEmailText, string StatusLabel)
{
    public static AdaptiveTrainingDueExceptionView ToAdaptiveTrainingDueExceptionView(AdaptiveTrainingDueException item)
    {
        return new AdaptiveTrainingDueExceptionView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTrainingDueExceptionView> ToAdaptiveTrainingDueExceptionViews(List<AdaptiveTrainingDueException> items)
    {
        var views = new List<AdaptiveTrainingDueExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTrainingDueExceptionView(item));
        }
        return views;
    }

    public static double SumAdaptiveTrainingDueExceptionView(AdaptiveTrainingDueExceptionView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedCost + parsedMaximum;
    }
}
