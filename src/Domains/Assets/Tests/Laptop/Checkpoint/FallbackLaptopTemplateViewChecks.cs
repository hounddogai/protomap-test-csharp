namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackLaptopTemplateViewChecks
{
    public static void FallbackLaptopTemplateViewShowsTheStatusInCapitals()
    {
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
        var view = FallbackLaptopTemplateView.ToFallbackLaptopTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackLaptopTemplateView.ToFallbackLaptopTemplateViews(new List<FallbackLaptopTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackLaptopTemplateViewShowsTheStatusInCapitals();
    }
}
