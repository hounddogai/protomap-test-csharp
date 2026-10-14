namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay profile.</summary>
public sealed record RetroPayProfileView(string Code, string Title, string Priority, string CeilingText, string Weight, string StatusLabel)
{
    public static RetroPayProfileView ToRetroPayProfileView(RetroPayProfile item)
    {
        return new RetroPayProfileView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayProfileView> ToRetroPayProfileViews(List<RetroPayProfile> items)
    {
        var views = new List<RetroPayProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayProfileView(item));
        }
        return views;
    }

    public static double SumRetroPayProfileView(RetroPayProfileView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCeiling + parsedWeight;
    }
}
