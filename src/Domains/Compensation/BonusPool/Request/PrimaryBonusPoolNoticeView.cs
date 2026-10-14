namespace Acme.Workforce.Compensation.BonusPool.Request;

using System.Globalization;

/// <summary>How screens and exports show a primary bonus pool notice.</summary>
public sealed record PrimaryBonusPoolNoticeView(string CodeText, string Label, string Capacity, string Title, string ActiveText, string StatusLabel)
{
    public static PrimaryBonusPoolNoticeView ToPrimaryBonusPoolNoticeView(PrimaryBonusPoolNotice item)
    {
        return new PrimaryBonusPoolNoticeView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryBonusPoolNoticeView> ToPrimaryBonusPoolNoticeViews(List<PrimaryBonusPoolNotice> items)
    {
        var views = new List<PrimaryBonusPoolNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryBonusPoolNoticeView(item));
        }
        return views;
    }

    public static double SumPrimaryBonusPoolNoticeView(PrimaryBonusPoolNoticeView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
