namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a capped license seat cycle.</summary>
public sealed record CappedLicenseSeatCycleView(string Code, string Name, string EndsAt, string Pinned, string CategoryText, string StatusLabel)
{
    public static CappedLicenseSeatCycleView ToCappedLicenseSeatCycleView(CappedLicenseSeatCycle item)
    {
        return new CappedLicenseSeatCycleView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLicenseSeatCycleView> ToCappedLicenseSeatCycleViews(List<CappedLicenseSeatCycle> items)
    {
        var views = new List<CappedLicenseSeatCycleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLicenseSeatCycleView(item));
        }
        return views;
    }

    public static double SumCappedLicenseSeatCycleView(CappedLicenseSeatCycleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
