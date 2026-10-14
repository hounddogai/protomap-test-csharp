namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global license seat template.</summary>
public sealed record GlobalLicenseSeatTemplateView(string Code, string NameText, string Rating, string CapacityText, string Checksum, string ExpiresAt, string StatusLabel)
{
    public static GlobalLicenseSeatTemplateView ToGlobalLicenseSeatTemplateView(GlobalLicenseSeatTemplate item)
    {
        return new GlobalLicenseSeatTemplateView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalLicenseSeatTemplateView> ToGlobalLicenseSeatTemplateViews(List<GlobalLicenseSeatTemplate> items)
    {
        var views = new List<GlobalLicenseSeatTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalLicenseSeatTemplateView(item));
        }
        return views;
    }

    public static double SumGlobalLicenseSeatTemplateView(GlobalLicenseSeatTemplateView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCapacity + (double)parsedExpiresAt;
    }
}
