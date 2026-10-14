namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAgreementViewChecks
{
    public static void CostModelAgreementViewShowsTheStatusInCapitals()
    {
        var item = CostModelAgreement.NewCostModelAgreement("code-344", "name-143");
        var view = CostModelAgreementView.ToCostModelAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelAgreementView.ToCostModelAgreementViews(new List<CostModelAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelAgreementViewShowsTheStatusInCapitals();
    }
}
