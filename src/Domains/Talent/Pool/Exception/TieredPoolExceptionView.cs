namespace Acme.Workforce.Talent.Pool.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pool exception.</summary>
public sealed record TieredPoolExceptionView(string Code, string Name, string BatchSize, string Archived, string Rate, string Skill, string Department, string StatusLabel)
{
    public static TieredPoolExceptionView ToTieredPoolExceptionView(TieredPoolException item)
    {
        return new TieredPoolExceptionView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPoolExceptionView> ToTieredPoolExceptionViews(List<TieredPoolException> items)
    {
        var views = new List<TieredPoolExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPoolExceptionView(item));
        }
        return views;
    }

    public static double SumTieredPoolExceptionView(TieredPoolExceptionView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRate;
    }
}
