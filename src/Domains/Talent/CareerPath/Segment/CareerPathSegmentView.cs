namespace Acme.Workforce.Talent.CareerPath.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path segment.</summary>
public sealed record CareerPathSegmentView(string Code, string NameText, string RevisionText, string CostCenter, string Team, string GradeText, string CeilingText, string StartsAt, string StatusLabel)
{
    public static CareerPathSegmentView ToCareerPathSegmentView(CareerPathSegment item)
    {
        return new CareerPathSegmentView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Team,
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathSegmentView> ToCareerPathSegmentViews(List<CareerPathSegment> items)
    {
        var views = new List<CareerPathSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathSegmentView(item));
        }
        return views;
    }

    public static double SumCareerPathSegmentView(CareerPathSegmentView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedCeiling + (double)parsedStartsAt;
    }
}
