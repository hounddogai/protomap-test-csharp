namespace Acme.Workforce.Talent.CareerPath.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary career path digest.</summary>
public sealed record PrimaryCareerPathDigestView(string Code, string Label, string Department, string CreatedAt, string BatchSize, string Margin, string SalaryBand, string StatusLabel)
{
    public static PrimaryCareerPathDigestView ToPrimaryCareerPathDigestView(PrimaryCareerPathDigest item)
    {
        return new PrimaryCareerPathDigestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Department, 120),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryCareerPathDigestView> ToPrimaryCareerPathDigestViews(List<PrimaryCareerPathDigest> items)
    {
        var views = new List<PrimaryCareerPathDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryCareerPathDigestView(item));
        }
        return views;
    }

    public static double SumPrimaryCareerPathDigestView(PrimaryCareerPathDigestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedBatchSize + parsedMargin;
    }
}
