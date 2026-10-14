namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment profile.</summary>
public sealed record GarnishmentProfileView(string Code, string TitleText, string EndsAt, string UpdatedAt, string ExternalRefText, string StatusLabel)
{
    public static GarnishmentProfileView ToGarnishmentProfileView(GarnishmentProfile item)
    {
        return new GarnishmentProfileView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentProfileView> ToGarnishmentProfileViews(List<GarnishmentProfile> items)
    {
        var views = new List<GarnishmentProfileView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentProfileView(item));
        }
        return views;
    }

    public static double SumGarnishmentProfileView(GarnishmentProfileView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedUpdatedAt;
    }
}
