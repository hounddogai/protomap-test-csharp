namespace Acme.Workforce.Talent.CareerPath.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path group.</summary>
public sealed record CareerPathGroupView(string Code, string Label, string PhoneText, string UpdatedAtText, string Currency, string Archived, string ExpiresAt, string Approved, string Version, string CeilingText, string StatusLabel)
{
    public static CareerPathGroupView ToCareerPathGroupView(CareerPathGroup item)
    {
        return new CareerPathGroupView(
            Text.Truncate(item.Code, 64),
            item.Label,
            Text.Truncate(item.Phone, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Archived}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathGroupView> ToCareerPathGroupViews(List<CareerPathGroup> items)
    {
        var views = new List<CareerPathGroupView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathGroupView(item));
        }
        return views;
    }

    public static double SumCareerPathGroupView(CareerPathGroupView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedExpiresAt + (double)parsedVersion;
    }
}
