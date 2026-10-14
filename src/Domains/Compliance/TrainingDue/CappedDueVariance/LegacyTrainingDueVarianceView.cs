namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy training due variance.</summary>
public sealed record LegacyTrainingDueVarianceView(string CodeText, string LabelText, string Ratio, string OwnerId, string Capacity, string Rating, string Currency, string EndsAtText, string ChannelText, string StatusLabel)
{
    public static LegacyTrainingDueVarianceView ToLegacyTrainingDueVarianceView(LegacyTrainingDueVariance item)
    {
        return new LegacyTrainingDueVarianceView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTrainingDueVarianceView> ToLegacyTrainingDueVarianceViews(List<LegacyTrainingDueVariance> items)
    {
        var views = new List<LegacyTrainingDueVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTrainingDueVarianceView(item));
        }
        return views;
    }

    public static double SumLegacyTrainingDueVarianceView(LegacyTrainingDueVarianceView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedCapacity + parsedRating;
    }
}
