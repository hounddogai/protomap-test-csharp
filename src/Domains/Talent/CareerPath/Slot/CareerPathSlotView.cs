namespace Acme.Workforce.Talent.CareerPath.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path slot.</summary>
public sealed record CareerPathSlotView(string Code, string Title, string Sequence, string TenureMonths, string JobCode, string SummaryText, string CountryText, string SalaryBandText, string Budget, string StatusLabel)
{
    public static CareerPathSlotView ToCareerPathSlotView(CareerPathSlot item)
    {
        return new CareerPathSlotView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Summary,
            item.Country,
            item.SalaryBand,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathSlotView> ToCareerPathSlotViews(List<CareerPathSlot> items)
    {
        var views = new List<CareerPathSlotView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathSlotView(item));
        }
        return views;
    }

    public static double SumCareerPathSlotView(CareerPathSlotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedTenureMonths + parsedBudget;
    }
}
