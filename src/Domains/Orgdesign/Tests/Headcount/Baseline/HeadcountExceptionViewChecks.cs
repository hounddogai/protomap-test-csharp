namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountExceptionViewChecks
{
    public static void HeadcountExceptionViewShowsTheStatusInCapitals()
    {
        var item = HeadcountException.NewHeadcountException("code-229", "name-155");
        var view = HeadcountExceptionView.ToHeadcountExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountExceptionView.ToHeadcountExceptionViews(new List<HeadcountException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountExceptionViewShowsTheStatusInCapitals();
    }
}
