namespace Acme.Workforce.Talent.Pool.PoolRule;

using System.Globalization;

/// <summary>How screens and exports show a pool exception.</summary>
public sealed record PoolExceptionView(string Code, string TitleText, string TimezoneText, string EmployeeIdText, string Description, string Quantity, string StatusLabel)
{
    public static PoolExceptionView ToPoolExceptionView(PoolException item)
    {
        return new PoolExceptionView(
            item.Code,
            item.Title,
            item.Timezone,
            item.EmployeeId,
            item.Description,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolExceptionView> ToPoolExceptionViews(List<PoolException> items)
    {
        var views = new List<PoolExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToPoolExceptionView(item));
        }
        return views;
    }

    public static double SumPoolExceptionView(PoolExceptionView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
