namespace Acme.Workforce.Absence.Policy.Score;

using System.Globalization;

/// <summary>How screens and exports show a policy score.</summary>
public sealed record PolicyScoreView(string CodeText, string Name, string Hours, string Description, string Channel, string Ceiling, string GraceDays, string StatusLabel)
{
    public static PolicyScoreView ToPolicyScoreView(PolicyScore item)
    {
        return new PolicyScoreView(
            item.Code,
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Channel,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyScoreView> ToPolicyScoreViews(List<PolicyScore> items)
    {
        var views = new List<PolicyScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyScoreView(item));
        }
        return views;
    }

    public static double SumPolicyScoreView(PolicyScoreView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedHours + parsedCeiling + (double)parsedGraceDays;
    }
}
