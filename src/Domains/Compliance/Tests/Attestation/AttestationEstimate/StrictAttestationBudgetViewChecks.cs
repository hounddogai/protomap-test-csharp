namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationBudgetViewChecks
{
    public static void StrictAttestationBudgetViewShowsTheStatusInCapitals()
    {
        var item = StrictAttestationBudget.NewStrictAttestationBudget("code-491", "label-945");
        var view = StrictAttestationBudgetView.ToStrictAttestationBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAttestationBudgetView.ToStrictAttestationBudgetViews(new List<StrictAttestationBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAttestationBudgetViewShowsTheStatusInCapitals();
    }
}
