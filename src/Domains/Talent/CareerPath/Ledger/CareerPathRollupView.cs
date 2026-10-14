namespace Acme.Workforce.Talent.CareerPath.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path rollup.</summary>
public sealed record CareerPathRollupView(string Code, string Label, string CostCenter, string Locale, string Region, string ManagerIdText, string RevisionText, string Unit, string CeilingText, string Sequence, string StatusLabel)
{
    public static CareerPathRollupView ToCareerPathRollupView(CareerPathRollup item)
    {
        return new CareerPathRollupView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 64),
            item.Locale,
            item.Region,
            item.ManagerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathRollupView> ToCareerPathRollupViews(List<CareerPathRollup> items)
    {
        var views = new List<CareerPathRollupView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathRollupView(item));
        }
        return views;
    }

    public static double SumCareerPathRollupView(CareerPathRollupView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedCeiling + (double)parsedSequence;
    }
}
