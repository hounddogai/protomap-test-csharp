namespace Acme.Workforce.Talent.Mobility.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient mobility request.</summary>
public sealed record LenientMobilityRequestView(string Code, string Name, string CostCenterText, string PinnedText, string Ratio, string Phone, string Checksum, string Priority, string JobCodeText, string Fte, string StatusLabel)
{
    public static LenientMobilityRequestView ToLenientMobilityRequestView(LenientMobilityRequest item)
    {
        return new LenientMobilityRequestView(
            item.Code,
            item.Name,
            item.CostCenter,
            $"{item.Pinned}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            Text.Truncate(item.Checksum, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMobilityRequestView> ToLenientMobilityRequestViews(List<LenientMobilityRequest> items)
    {
        var views = new List<LenientMobilityRequestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMobilityRequestView(item));
        }
        return views;
    }

    public static double SumLenientMobilityRequestView(LenientMobilityRequestView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedPriority + parsedFte;
    }
}
