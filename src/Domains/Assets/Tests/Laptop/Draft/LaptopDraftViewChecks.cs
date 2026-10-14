namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopDraftViewChecks
{
    public static void LaptopDraftViewShowsTheStatusInCapitals()
    {
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
        var view = LaptopDraftView.ToLaptopDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopDraftView.ToLaptopDraftViews(new List<LaptopDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopDraftViewShowsTheStatusInCapitals();
    }
}
