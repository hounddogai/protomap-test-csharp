namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception claim.</summary>
public sealed record ExceptionClaimView(string CodeText, string Name, string Channel, string CapacityText, string Version, string CreatedAt, string Reviewer, string UnitText, string Pinned, string ContactEmailText, string Fte, string StatusLabel)
{
    public static ExceptionClaimView ToExceptionClaimView(ExceptionClaim item)
    {
        return new ExceptionClaimView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Channel,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.Unit, 80),
            $"{item.Pinned}",
            Text.Truncate(item.ContactEmail, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionClaimView> ToExceptionClaimViews(List<ExceptionClaim> items)
    {
        var views = new List<ExceptionClaimView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionClaimView(item));
        }
        return views;
    }

    public static double SumExceptionClaimView(ExceptionClaimView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedVersion + (double)parsedCreatedAt;
    }
}
