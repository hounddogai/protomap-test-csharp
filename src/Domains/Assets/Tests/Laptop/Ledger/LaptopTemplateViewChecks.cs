namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopTemplateViewChecks
{
    public static void LaptopTemplateViewShowsTheStatusInCapitals()
    {
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
        var view = LaptopTemplateView.ToLaptopTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopTemplateView.ToLaptopTemplateViews(new List<LaptopTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopTemplateViewShowsTheStatusInCapitals();
    }
}
