namespace Acme.Workforce.Orgdesign.Position.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary position profile.</summary>
public sealed record PrimaryPositionProfileView(string CodeText, string Title, string ManagerId, string Minimum, string Summary, string Priority, string JobCodeText, string StatusLabel)
{
    public static PrimaryPositionProfileView ToPrimaryPositionProfileView(PrimaryPositionProfile item)
    {
        return new PrimaryPositionProfileView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.ManagerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPositionProfileView> ToPrimaryPositionProfileViews(List<PrimaryPositionProfile> items)
    {
        var views = new List<PrimaryPositionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPositionProfileView(item));
        }
        return views;
    }

    public static double SumPrimaryPositionProfileView(PrimaryPositionProfileView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedPriority;
    }
}
