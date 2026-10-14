namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopExceptionViewChecks
{
    public static void LaptopExceptionViewShowsTheStatusInCapitals()
    {
        var item = LaptopException.NewLaptopException("code-154", "label-748");
        var view = LaptopExceptionView.ToLaptopExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopExceptionView.ToLaptopExceptionViews(new List<LaptopException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopExceptionViewShowsTheStatusInCapitals();
    }
}
