namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle exception.</summary>
public sealed record MeritCycleExceptionView(string CodeText, string Title, string Checksum, string Active, string Notes, string Hours, string Phone, string StartsAt, string Name, string Amount, string Pinned, string StatusLabel)
{
    public static MeritCycleExceptionView ToMeritCycleExceptionView(MeritCycleException item)
    {
        return new MeritCycleExceptionView(
            item.Code,
            item.Title,
            item.Checksum,
            $"{item.Active}",
            Text.Truncate(item.Notes, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleExceptionView> ToMeritCycleExceptionViews(List<MeritCycleException> items)
    {
        var views = new List<MeritCycleExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleExceptionView(item));
        }
        return views;
    }

    public static double SumMeritCycleExceptionView(MeritCycleExceptionView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedStartsAt + parsedAmount;
    }
}
