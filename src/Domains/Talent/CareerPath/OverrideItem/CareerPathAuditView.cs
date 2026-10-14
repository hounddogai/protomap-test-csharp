namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path audit.</summary>
public sealed record CareerPathAuditView(string CodeText, string Label, string PinnedText, string Title, string Fte, string SourceText, string Sequence, string EmployeeId, string Maximum, string StatusLabel)
{
    public static CareerPathAuditView ToCareerPathAuditView(CareerPathAudit item)
    {
        return new CareerPathAuditView(
            item.Code,
            Text.Truncate(item.Label, 80),
            $"{item.Pinned}",
            Text.Truncate(item.Title, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathAuditView> ToCareerPathAuditViews(List<CareerPathAudit> items)
    {
        var views = new List<CareerPathAuditView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathAuditView(item));
        }
        return views;
    }

    public static double SumCareerPathAuditView(CareerPathAuditView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedSequence + parsedMaximum;
    }
}
