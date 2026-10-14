namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WarrantyRuleViewChecks
{
    public static void WarrantyRuleViewShowsTheStatusInCapitals()
    {
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
        var view = WarrantyRuleView.ToWarrantyRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyRuleView.ToWarrantyRuleViews(new List<WarrantyRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyRuleViewShowsTheStatusInCapitals();
    }
}
