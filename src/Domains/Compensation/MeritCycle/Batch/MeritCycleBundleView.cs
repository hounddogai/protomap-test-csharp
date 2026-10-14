namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle bundle.</summary>
public sealed record MeritCycleBundleView(string CodeText, string Name, string Title, string Department, string Archived, string Approved, string CreatedAt, string Priority, string Maximum, string SkillText, string Budget, string StatusLabel)
{
    public static MeritCycleBundleView ToMeritCycleBundleView(MeritCycleBundle item)
    {
        return new MeritCycleBundleView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Department, 80),
            $"{item.Archived}",
            $"{item.Approved}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleBundleView> ToMeritCycleBundleViews(List<MeritCycleBundle> items)
    {
        var views = new List<MeritCycleBundleView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleBundleView(item));
        }
        return views;
    }

    public static double SumMeritCycleBundleView(MeritCycleBundleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedPriority + parsedMaximum;
    }
}
