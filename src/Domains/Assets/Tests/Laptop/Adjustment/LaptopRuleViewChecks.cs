namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleViewChecks
{
    public static void LaptopRuleViewShowsTheStatusInCapitals()
    {
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
        var view = LaptopRuleView.ToLaptopRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopRuleView.ToLaptopRuleViews(new List<LaptopRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopRuleViewShowsTheStatusInCapitals();
    }
}
