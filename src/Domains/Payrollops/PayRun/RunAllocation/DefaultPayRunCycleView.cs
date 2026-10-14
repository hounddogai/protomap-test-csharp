namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pay run cycle.</summary>
public sealed record DefaultPayRunCycleView(string Code, string Title, string JobCode, string Rate, string OwnerIdText, string StatusLabel)
{
    public static DefaultPayRunCycleView ToDefaultPayRunCycleView(DefaultPayRunCycle item)
    {
        return new DefaultPayRunCycleView(
            item.Code,
            item.Title,
            Text.Truncate(item.JobCode, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayRunCycleView> ToDefaultPayRunCycleViews(List<DefaultPayRunCycle> items)
    {
        var views = new List<DefaultPayRunCycleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayRunCycleView(item));
        }
        return views;
    }

    public static double SumDefaultPayRunCycleView(DefaultPayRunCycleView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
