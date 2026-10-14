namespace Acme.Workforce.Talent.Nomination.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback nomination exception.</summary>
public sealed record FallbackNominationExceptionView(string Code, string Name, string Channel, string UpdatedAt, string BadgeText, string DepartmentText, string StatusLabel)
{
    public static FallbackNominationExceptionView ToFallbackNominationExceptionView(FallbackNominationException item)
    {
        return new FallbackNominationExceptionView(
            item.Code,
            item.Name,
            item.Channel,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Department, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackNominationExceptionView> ToFallbackNominationExceptionViews(List<FallbackNominationException> items)
    {
        var views = new List<FallbackNominationExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackNominationExceptionView(item));
        }
        return views;
    }

    public static double SumFallbackNominationExceptionView(FallbackNominationExceptionView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
