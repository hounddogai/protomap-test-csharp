namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack template.</summary>
public sealed record PolicyAckTemplateView(string CodeText, string Name, string Pinned, string EndsAt, string Retries, string Version, string Fte, string Attempts, string SalaryBand, string EmployeeIdText, string RateText, string StatusLabel)
{
    public static PolicyAckTemplateView ToPolicyAckTemplateView(PolicyAckTemplate item)
    {
        return new PolicyAckTemplateView(
            Text.Truncate(item.Code, 40),
            item.Name,
            $"{item.Pinned}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.EmployeeId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckTemplateView> ToPolicyAckTemplateViews(List<PolicyAckTemplate> items)
    {
        var views = new List<PolicyAckTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckTemplateView(item));
        }
        return views;
    }

    public static double SumPolicyAckTemplateView(PolicyAckTemplateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + (double)parsedVersion;
    }
}
