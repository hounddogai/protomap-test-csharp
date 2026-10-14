namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario draft.</summary>
public sealed record ScenarioDraftView(string CodeText, string Name, string Capacity, string BatchSize, string TeamText, string ChannelText, string ManagerId, string StatusLabel)
{
    public static ScenarioDraftView ToScenarioDraftView(ScenarioDraft item)
    {
        return new ScenarioDraftView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Channel,
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioDraftView> ToScenarioDraftViews(List<ScenarioDraft> items)
    {
        var views = new List<ScenarioDraftView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioDraftView(item));
        }
        return views;
    }

    public static double SumScenarioDraftView(ScenarioDraftView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedBatchSize;
    }
}
