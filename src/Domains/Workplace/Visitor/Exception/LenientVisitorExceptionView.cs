namespace Acme.Workforce.Workplace.Visitor.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient visitor exception.</summary>
public sealed record LenientVisitorExceptionView(string Code, string Name, string Ratio, string RetriesText, string Badge, string StatusLabel)
{
    public static LenientVisitorExceptionView ToLenientVisitorExceptionView(LenientVisitorException item)
    {
        return new LenientVisitorExceptionView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientVisitorExceptionView> ToLenientVisitorExceptionViews(List<LenientVisitorException> items)
    {
        var views = new List<LenientVisitorExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLenientVisitorExceptionView(item));
        }
        return views;
    }

    public static double SumLenientVisitorExceptionView(LenientVisitorExceptionView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRetries;
    }
}
