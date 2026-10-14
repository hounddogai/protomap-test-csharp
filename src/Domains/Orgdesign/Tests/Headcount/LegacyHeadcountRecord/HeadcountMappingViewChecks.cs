namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class HeadcountMappingViewChecks
{
    public static void HeadcountMappingViewShowsTheStatusInCapitals()
    {
        var item = HeadcountMapping.NewHeadcountMapping("code-972", "label-124");
        var view = HeadcountMappingView.ToHeadcountMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountMappingView.ToHeadcountMappingViews(new List<HeadcountMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountMappingViewShowsTheStatusInCapitals();
    }
}
