namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using System.Globalization;

/// <summary>How screens and exports show a garnishment digest.</summary>
public sealed record GarnishmentDigestView(string Code, string Label, string CreatedAt, string ExpiresAtText, string WeightText, string Location, string Active, string StatusLabel)
{
    public static GarnishmentDigestView ToGarnishmentDigestView(GarnishmentDigest item)
    {
        return new GarnishmentDigestView(
            item.Code,
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentDigestView> ToGarnishmentDigestViews(List<GarnishmentDigest> items)
    {
        var views = new List<GarnishmentDigestView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentDigestView(item));
        }
        return views;
    }

    public static double SumGarnishmentDigestView(GarnishmentDigestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedExpiresAt + parsedWeight;
    }
}
