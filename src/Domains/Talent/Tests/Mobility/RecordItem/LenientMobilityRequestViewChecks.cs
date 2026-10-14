namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientMobilityRequestViewChecks
{
    public static void LenientMobilityRequestViewShowsTheStatusInCapitals()
    {
        var item = LenientMobilityRequest.NewLenientMobilityRequest("code-775", "name-923");
        var view = LenientMobilityRequestView.ToLenientMobilityRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientMobilityRequestView.ToLenientMobilityRequestViews(new List<LenientMobilityRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientMobilityRequestViewShowsTheStatusInCapitals();
    }
}
