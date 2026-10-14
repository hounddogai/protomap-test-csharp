namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a primary retro pay audit.</summary>
public sealed record PrimaryRetroPayAuditView(string Code, string Label, string TimezoneText, string Ratio, string StatusLabel)
{
    public static PrimaryRetroPayAuditView ToPrimaryRetroPayAuditView(PrimaryRetroPayAudit item)
    {
        return new PrimaryRetroPayAuditView(
            item.Code,
            item.Label,
            item.Timezone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRetroPayAuditView> ToPrimaryRetroPayAuditViews(List<PrimaryRetroPayAudit> items)
    {
        var views = new List<PrimaryRetroPayAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRetroPayAuditView(item));
        }
        return views;
    }

    public static double SumPrimaryRetroPayAuditView(PrimaryRetroPayAuditView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
