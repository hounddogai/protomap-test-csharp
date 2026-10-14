namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using System.Globalization;

/// <summary>How screens and exports show a cost model exception.</summary>
public sealed record CostModelExceptionView(string Code, string Name, string Label, string Locale, string ApprovedText, string EmployeeId, string Version, string Minimum, string Hours, string JobCode, string StatusLabel)
{
    public static CostModelExceptionView ToCostModelExceptionView(CostModelException item)
    {
        return new CostModelExceptionView(
            item.Code,
            item.Name,
            item.Label,
            item.Locale,
            $"{item.Approved}",
            item.EmployeeId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelExceptionView> ToCostModelExceptionViews(List<CostModelException> items)
    {
        var views = new List<CostModelExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelExceptionView(item));
        }
        return views;
    }

    public static double SumCostModelExceptionView(CostModelExceptionView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMinimum + parsedHours;
    }
}
