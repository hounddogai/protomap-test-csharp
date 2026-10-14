namespace Acme.Workforce.Compliance.Exception.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a exception cycle.</summary>
public sealed record ExceptionCycleView(string Code, string Label, string Maximum, string Retries, string CreatedAtText, string WindowDays, string StatusLabel)
{
    public static ExceptionCycleView ToExceptionCycleView(ExceptionCycle item)
    {
        return new ExceptionCycleView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionCycleView> ToExceptionCycleViews(List<ExceptionCycle> items)
    {
        var views = new List<ExceptionCycleView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionCycleView(item));
        }
        return views;
    }

    public static double SumExceptionCycleView(ExceptionCycleView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedRetries + (double)parsedCreatedAt;
    }
}
