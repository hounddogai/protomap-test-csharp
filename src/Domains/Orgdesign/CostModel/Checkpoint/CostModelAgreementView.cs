namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a cost model agreement.</summary>
public sealed record CostModelAgreementView(string CodeText, string Name, string Attempts, string Budget, string Amount, string StatusLabel)
{
    public static CostModelAgreementView ToCostModelAgreementView(CostModelAgreement item)
    {
        return new CostModelAgreementView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelAgreementView> ToCostModelAgreementViews(List<CostModelAgreement> items)
    {
        var views = new List<CostModelAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelAgreementView(item));
        }
        return views;
    }

    public static double SumCostModelAgreementView(CostModelAgreementView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedBudget + parsedAmount;
    }
}
