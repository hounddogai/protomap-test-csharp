namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a lenient scenario agreement.</summary>
public sealed record LenientScenarioAgreementView(string Code, string Label, string Visible, string Summary, string PinnedText, string Amount, string StatusLabel)
{
    public static LenientScenarioAgreementView ToLenientScenarioAgreementView(LenientScenarioAgreement item)
    {
        return new LenientScenarioAgreementView(
            item.Code,
            item.Label,
            $"{item.Visible}",
            item.Summary,
            $"{item.Pinned}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientScenarioAgreementView> ToLenientScenarioAgreementViews(List<LenientScenarioAgreement> items)
    {
        var views = new List<LenientScenarioAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLenientScenarioAgreementView(item));
        }
        return views;
    }

    public static double SumLenientScenarioAgreementView(LenientScenarioAgreementView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
