namespace Acme.Workforce.Absence.Balance.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance digest.</summary>
public sealed record BalanceDigestView(string Code, string TitleText, string Capacity, string AttemptsText, string SegmentText, string FloorValue, string StatusLabel)
{
    public static BalanceDigestView ToBalanceDigestView(BalanceDigest item)
    {
        return new BalanceDigestView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceDigestView> ToBalanceDigestViews(List<BalanceDigest> items)
    {
        var views = new List<BalanceDigestView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceDigestView(item));
        }
        return views;
    }

    public static double SumBalanceDigestView(BalanceDigestView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedAttempts + parsedFloorValue;
    }
}
