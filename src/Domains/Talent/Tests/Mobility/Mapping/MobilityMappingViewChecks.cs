namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MobilityMappingViewChecks
{
    public static void MobilityMappingViewShowsTheStatusInCapitals()
    {
        var item = MobilityMapping.NewMobilityMapping("code-897", "title-357");
        var view = MobilityMappingView.ToMobilityMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityMappingView.ToMobilityMappingViews(new List<MobilityMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityMappingViewShowsTheStatusInCapitals();
    }
}
