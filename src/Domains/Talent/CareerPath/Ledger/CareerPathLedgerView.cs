namespace Acme.Workforce.Talent.CareerPath.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path ledger.</summary>
public sealed record CareerPathLedgerView(string Code, string Label, string JobCode, string Weight, string StartsAt, string CategoryText, string Name, string StatusLabel)
{
    public static CareerPathLedgerView ToCareerPathLedgerView(CareerPathLedger item)
    {
        return new CareerPathLedgerView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.JobCode, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathLedgerView> ToCareerPathLedgerViews(List<CareerPathLedger> items)
    {
        var views = new List<CareerPathLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathLedgerView(item));
        }
        return views;
    }

    public static double SumCareerPathLedgerView(CareerPathLedgerView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedStartsAt;
    }
}
