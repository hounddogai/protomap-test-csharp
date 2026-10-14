namespace Acme.Workforce.Talent.Mobility.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility target.</summary>
public sealed record MobilityTargetView(string Code, string Name, string LocaleText, string Approved, string Retries, string OwnerIdText, string Label, string StatusLabel)
{
    public static MobilityTargetView ToMobilityTargetView(MobilityTarget item)
    {
        return new MobilityTargetView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Locale,
            $"{item.Approved}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Label, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityTargetView> ToMobilityTargetViews(List<MobilityTarget> items)
    {
        var views = new List<MobilityTargetView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityTargetView(item));
        }
        return views;
    }

    public static double SumMobilityTargetView(MobilityTargetView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
