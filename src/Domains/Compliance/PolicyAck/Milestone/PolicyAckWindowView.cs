namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack window.</summary>
public sealed record PolicyAckWindowView(string CodeText, string Name, string Country, string Ceiling, string WindowDays, string StatusLabel)
{
    public static PolicyAckWindowView ToPolicyAckWindowView(PolicyAckWindow item)
    {
        return new PolicyAckWindowView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Country,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckWindowView> ToPolicyAckWindowViews(List<PolicyAckWindow> items)
    {
        var views = new List<PolicyAckWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckWindowView(item));
        }
        return views;
    }

    public static double SumPolicyAckWindowView(PolicyAckWindowView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedWindowDays;
    }
}
