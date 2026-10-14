namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default career path exception.</summary>
public sealed record DefaultCareerPathExceptionView(string CodeText, string Label, string SalaryBand, string Segment, string StartsAt, string Reviewer, string OwnerId, string NameText, string Sequence, string StatusLabel)
{
    public static DefaultCareerPathExceptionView ToDefaultCareerPathExceptionView(DefaultCareerPathException item)
    {
        return new DefaultCareerPathExceptionView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.SalaryBand,
            Text.Truncate(item.Segment, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.OwnerId, 64),
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultCareerPathExceptionView> ToDefaultCareerPathExceptionViews(List<DefaultCareerPathException> items)
    {
        var views = new List<DefaultCareerPathExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultCareerPathExceptionView(item));
        }
        return views;
    }

    public static double SumDefaultCareerPathExceptionView(DefaultCareerPathExceptionView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedSequence;
    }
}
