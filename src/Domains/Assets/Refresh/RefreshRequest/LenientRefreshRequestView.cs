namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient refresh request.</summary>
public sealed record LenientRefreshRequestView(string Code, string Title, string Quantity, string Hours, string EmployeeIdText, string ManagerIdText, string StatusLabel)
{
    public static LenientRefreshRequestView ToLenientRefreshRequestView(LenientRefreshRequest item)
    {
        return new LenientRefreshRequestView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRefreshRequestView> ToLenientRefreshRequestViews(List<LenientRefreshRequest> items)
    {
        var views = new List<LenientRefreshRequestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRefreshRequestView(item));
        }
        return views;
    }

    public static double SumLenientRefreshRequestView(LenientRefreshRequestView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedHours;
    }
}
