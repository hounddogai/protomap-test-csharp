namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pay run digest.</summary>
public sealed record RollingPayRunDigestView(string Code, string TitleText, string TenureMonths, string PriorityText, string ContactEmail, string LocationText, string RevisionText, string FteText, string CountryText, string StatusLabel)
{
    public static RollingPayRunDigestView ToRollingPayRunDigestView(RollingPayRunDigest item)
    {
        return new RollingPayRunDigestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Location,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPayRunDigestView> ToRollingPayRunDigestViews(List<RollingPayRunDigest> items)
    {
        var views = new List<RollingPayRunDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPayRunDigestView(item));
        }
        return views;
    }

    public static double SumRollingPayRunDigestView(RollingPayRunDigestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedPriority + (double)parsedRevision;
    }
}
