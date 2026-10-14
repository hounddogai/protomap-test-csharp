namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using System.Globalization;

/// <summary>How screens and exports show a license seat digest.</summary>
public sealed record LicenseSeatDigestView(string Code, string Name, string Sequence, string Weight, string Cost, string StatusLabel)
{
    public static LicenseSeatDigestView ToLicenseSeatDigestView(LicenseSeatDigest item)
    {
        return new LicenseSeatDigestView(
            item.Code,
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatDigestView> ToLicenseSeatDigestViews(List<LicenseSeatDigest> items)
    {
        var views = new List<LicenseSeatDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatDigestView(item));
        }
        return views;
    }

    public static double SumLicenseSeatDigestView(LicenseSeatDigestView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedWeight + parsedCost;
    }
}
