namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient balance group.</summary>
public sealed record LenientBalanceGroupView(string Code, string NameText, string Hours, string DescriptionText, string FloorValueText, string BatchSizeText, string Team, string Retries, string CountryText, string ManagerId, string StatusLabel)
{
    public static LenientBalanceGroupView ToLenientBalanceGroupView(LenientBalanceGroup item)
    {
        return new LenientBalanceGroupView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBalanceGroupView> ToLenientBalanceGroupViews(List<LenientBalanceGroup> items)
    {
        var views = new List<LenientBalanceGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBalanceGroupView(item));
        }
        return views;
    }

    public static double SumLenientBalanceGroupView(LenientBalanceGroupView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedHours + parsedFloorValue + (double)parsedBatchSize;
    }
}
