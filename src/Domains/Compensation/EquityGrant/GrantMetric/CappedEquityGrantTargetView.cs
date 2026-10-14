namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using System.Globalization;

/// <summary>How screens and exports show a capped equity grant target.</summary>
public sealed record CappedEquityGrantTargetView(string Code, string NameText, string RateText, string TitleText, string EmployeeId, string StatusLabel)
{
    public static CappedEquityGrantTargetView ToCappedEquityGrantTargetView(CappedEquityGrantTarget item)
    {
        return new CappedEquityGrantTargetView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedEquityGrantTargetView> ToCappedEquityGrantTargetViews(List<CappedEquityGrantTarget> items)
    {
        var views = new List<CappedEquityGrantTargetView>();
        foreach (var item in items)
        {
            views.Add(ToCappedEquityGrantTargetView(item));
        }
        return views;
    }

    public static double SumCappedEquityGrantTargetView(CappedEquityGrantTargetView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
