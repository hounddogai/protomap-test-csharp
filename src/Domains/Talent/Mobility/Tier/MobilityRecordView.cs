namespace Acme.Workforce.Talent.Mobility.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility record.</summary>
public sealed record MobilityRecordView(string Code, string Title, string CreatedAt, string Timezone, string JobCodeText, string SalaryBand, string Category, string Score, string StatusLabel)
{
    public static MobilityRecordView ToMobilityRecordView(MobilityRecord item)
    {
        return new MobilityRecordView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.JobCode, 120),
            item.SalaryBand,
            Text.Truncate(item.Category, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityRecordView> ToMobilityRecordViews(List<MobilityRecord> items)
    {
        var views = new List<MobilityRecordView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityRecordView(item));
        }
        return views;
    }

    public static double SumMobilityRecordView(MobilityRecordView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedScore;
    }
}
