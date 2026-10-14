namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception milestone.</summary>
public sealed record ExceptionMilestoneView(string Code, string Title, string Version, string Fte, string EmployeeId, string StatusLabel)
{
    public static ExceptionMilestoneView ToExceptionMilestoneView(ExceptionMilestone item)
    {
        return new ExceptionMilestoneView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionMilestoneView> ToExceptionMilestoneViews(List<ExceptionMilestone> items)
    {
        var views = new List<ExceptionMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionMilestoneView(item));
        }
        return views;
    }

    public static double SumExceptionMilestoneView(ExceptionMilestoneView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedFte;
    }
}
