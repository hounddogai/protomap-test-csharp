namespace Acme.Workforce.Orgdesign.Span.Claim;

using System.Globalization;

/// <summary>How screens and exports show a rolling span exception.</summary>
public sealed record RollingSpanExceptionView(string CodeText, string Title, string LeaveDays, string EmployeeIdText, string Name, string Amount, string StatusLabel)
{
    public static RollingSpanExceptionView ToRollingSpanExceptionView(RollingSpanException item)
    {
        return new RollingSpanExceptionView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSpanExceptionView> ToRollingSpanExceptionViews(List<RollingSpanException> items)
    {
        var views = new List<RollingSpanExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSpanExceptionView(item));
        }
        return views;
    }

    public static double SumRollingSpanExceptionView(RollingSpanExceptionView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedAmount;
    }
}
