namespace Acme.Workforce.Compliance.Exception.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception manifest.</summary>
public sealed record ExceptionManifestView(string Code, string Title, string EmployeeId, string BatchSize, string Locale, string Grade, string Fte, string StatusLabel)
{
    public static ExceptionManifestView ToExceptionManifestView(ExceptionManifest item)
    {
        return new ExceptionManifestView(
            item.Code,
            item.Title,
            item.EmployeeId,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Grade, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionManifestView> ToExceptionManifestViews(List<ExceptionManifest> items)
    {
        var views = new List<ExceptionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionManifestView(item));
        }
        return views;
    }

    public static double SumExceptionManifestView(ExceptionManifestView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedFte;
    }
}
