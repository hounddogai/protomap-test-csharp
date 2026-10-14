namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default training due window.</summary>
public sealed record DefaultTrainingDueWindowView(string CodeText, string Name, string PhoneText, string Revision, string ContactEmail, string JobCode, string StatusLabel)
{
    public static DefaultTrainingDueWindowView ToDefaultTrainingDueWindowView(DefaultTrainingDueWindow item)
    {
        return new DefaultTrainingDueWindowView(
            item.Code,
            item.Name,
            Text.Truncate(item.Phone, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultTrainingDueWindowView> ToDefaultTrainingDueWindowViews(List<DefaultTrainingDueWindow> items)
    {
        var views = new List<DefaultTrainingDueWindowView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultTrainingDueWindowView(item));
        }
        return views;
    }

    public static double SumDefaultTrainingDueWindowView(DefaultTrainingDueWindowView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
