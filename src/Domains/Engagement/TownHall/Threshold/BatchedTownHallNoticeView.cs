namespace Acme.Workforce.Engagement.TownHall.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched town hall notice.</summary>
public sealed record BatchedTownHallNoticeView(string Code, string Name, string MinimumText, string WeightText, string Unit, string Pinned, string Channel, string JobCodeText, string ExternalRefText, string EndsAt, string StatusLabel)
{
    public static BatchedTownHallNoticeView ToBatchedTownHallNoticeView(BatchedTownHallNotice item)
    {
        return new BatchedTownHallNoticeView(
            item.Code,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Pinned}",
            Text.Truncate(item.Channel, 40),
            item.JobCode,
            item.ExternalRef,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTownHallNoticeView> ToBatchedTownHallNoticeViews(List<BatchedTownHallNotice> items)
    {
        var views = new List<BatchedTownHallNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTownHallNoticeView(item));
        }
        return views;
    }

    public static double SumBatchedTownHallNoticeView(BatchedTownHallNoticeView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedWeight + (double)parsedEndsAt;
    }
}
