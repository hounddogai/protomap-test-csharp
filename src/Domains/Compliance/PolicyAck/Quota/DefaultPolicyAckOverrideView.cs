namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default policy ack override.</summary>
public sealed record DefaultPolicyAckOverrideView(string CodeText, string LabelText, string MinimumText, string Summary, string MarginText, string Fte, string Discount, string EndsAt, string StatusLabel)
{
    public static DefaultPolicyAckOverrideView ToDefaultPolicyAckOverrideView(DefaultPolicyAckOverride item)
    {
        return new DefaultPolicyAckOverrideView(
            item.Code,
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPolicyAckOverrideView> ToDefaultPolicyAckOverrideViews(List<DefaultPolicyAckOverride> items)
    {
        var views = new List<DefaultPolicyAckOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPolicyAckOverrideView(item));
        }
        return views;
    }

    public static double SumDefaultPolicyAckOverrideView(DefaultPolicyAckOverrideView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedMargin + parsedFte;
    }
}
