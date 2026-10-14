namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount exception.</summary>
public sealed record HeadcountExceptionView(string Code, string NameText, string LabelText, string Team, string EndsAtText, string VersionText, string CeilingText, string CostCenterText, string StatusLabel)
{
    public static HeadcountExceptionView ToHeadcountExceptionView(HeadcountException item)
    {
        return new HeadcountExceptionView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Label,
            Text.Truncate(item.Team, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountExceptionView> ToHeadcountExceptionViews(List<HeadcountException> items)
    {
        var views = new List<HeadcountExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountExceptionView(item));
        }
        return views;
    }

    public static double SumHeadcountExceptionView(HeadcountExceptionView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedVersion + parsedCeiling;
    }
}
