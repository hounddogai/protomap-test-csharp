namespace Acme.Workforce.Assets.Assignment.Exception;

using System.Globalization;

/// <summary>How screens and exports show a regional assignment exception.</summary>
public sealed record RegionalAssignmentExceptionView(string Code, string Title, string ManagerId, string Channel, string Attempts, string JobCodeText, string Phone, string Department, string ExpiresAt, string Budget, string BatchSize, string StatusLabel)
{
    public static RegionalAssignmentExceptionView ToRegionalAssignmentExceptionView(RegionalAssignmentException item)
    {
        return new RegionalAssignmentExceptionView(
            item.Code,
            item.Title,
            item.ManagerId,
            item.Channel,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Phone,
            item.Department,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAssignmentExceptionView> ToRegionalAssignmentExceptionViews(List<RegionalAssignmentException> items)
    {
        var views = new List<RegionalAssignmentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAssignmentExceptionView(item));
        }
        return views;
    }

    public static double SumRegionalAssignmentExceptionView(RegionalAssignmentExceptionView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedExpiresAt + parsedBudget;
    }
}
