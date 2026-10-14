namespace Acme.Workforce.Talent.Mobility.Preference;

using System.Globalization;

/// <summary>How screens and exports show a default mobility claim.</summary>
public sealed record DefaultMobilityClaimView(string CodeText, string Name, string TeamText, string Currency, string MaximumText, string Quantity, string PinnedText, string StatusLabel)
{
    public static DefaultMobilityClaimView ToDefaultMobilityClaimView(DefaultMobilityClaim item)
    {
        return new DefaultMobilityClaimView(
            item.Code,
            item.Name,
            item.Team,
            item.Currency,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultMobilityClaimView> ToDefaultMobilityClaimViews(List<DefaultMobilityClaim> items)
    {
        var views = new List<DefaultMobilityClaimView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultMobilityClaimView(item));
        }
        return views;
    }

    public static double SumDefaultMobilityClaimView(DefaultMobilityClaimView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedQuantity;
    }
}
