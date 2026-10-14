namespace Acme.Workforce.Compliance.Certification.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification adjustment.</summary>
public sealed record CertificationAdjustmentView(string Code, string Label, string ThresholdText, string Skill, string Limit, string FloorValue, string Rate, string DurationMinutesText, string Title, string StatusLabel)
{
    public static CertificationAdjustmentView ToCertificationAdjustmentView(CertificationAdjustment item)
    {
        return new CertificationAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationAdjustmentView> ToCertificationAdjustmentViews(List<CertificationAdjustment> items)
    {
        var views = new List<CertificationAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationAdjustmentView(item));
        }
        return views;
    }

    public static double SumCertificationAdjustmentView(CertificationAdjustmentView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLimit + parsedFloorValue;
    }
}
