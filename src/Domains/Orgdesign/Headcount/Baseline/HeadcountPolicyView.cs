namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a headcount policy.</summary>
public sealed record HeadcountPolicyView(string Code, string Label, string ExpiresAt, string Channel, string Cost, string StatusLabel)
{
    public static HeadcountPolicyView ToHeadcountPolicyView(HeadcountPolicy item)
    {
        return new HeadcountPolicyView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountPolicyView> ToHeadcountPolicyViews(List<HeadcountPolicy> items)
    {
        var views = new List<HeadcountPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountPolicyView(item));
        }
        return views;
    }

    public static double SumHeadcountPolicyView(HeadcountPolicyView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedCost;
    }
}
