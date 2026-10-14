namespace Acme.Workforce.Compliance.Exception.Template;

using System.Globalization;

/// <summary>How screens and exports show a lenient exception variance.</summary>
public sealed record LenientExceptionVarianceView(string Code, string LabelText, string Team, string ManagerId, string GradeText, string Ceiling, string StatusLabel)
{
    public static LenientExceptionVarianceView ToLenientExceptionVarianceView(LenientExceptionVariance item)
    {
        return new LenientExceptionVarianceView(
            item.Code,
            item.Label,
            item.Team,
            item.ManagerId,
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientExceptionVarianceView> ToLenientExceptionVarianceViews(List<LenientExceptionVariance> items)
    {
        var views = new List<LenientExceptionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToLenientExceptionVarianceView(item));
        }
        return views;
    }

    public static double SumLenientExceptionVarianceView(LenientExceptionVarianceView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
