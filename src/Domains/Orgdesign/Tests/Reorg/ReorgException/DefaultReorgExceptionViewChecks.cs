namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class DefaultReorgExceptionViewChecks
{
    public static void DefaultReorgExceptionViewShowsTheStatusInCapitals()
    {
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
        var view = DefaultReorgExceptionView.ToDefaultReorgExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultReorgExceptionView.ToDefaultReorgExceptionViews(new List<DefaultReorgException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultReorgExceptionViewShowsTheStatusInCapitals();
    }
}
