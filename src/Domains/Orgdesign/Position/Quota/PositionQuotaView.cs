namespace Acme.Workforce.Orgdesign.Position.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position quota.</summary>
public sealed record PositionQuotaView(string CodeText, string Label, string Archived, string ChannelText, string JobCode, string Amount, string StatusLabel)
{
    public static PositionQuotaView ToPositionQuotaView(PositionQuota item)
    {
        return new PositionQuotaView(
            Text.Truncate(item.Code, 120),
            item.Label,
            $"{item.Archived}",
            Text.Truncate(item.Channel, 80),
            Text.Truncate(item.JobCode, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionQuotaView> ToPositionQuotaViews(List<PositionQuota> items)
    {
        var views = new List<PositionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPositionQuotaView(item));
        }
        return views;
    }

    public static double SumPositionQuotaView(PositionQuotaView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
