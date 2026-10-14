namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StrictSpanProposalViewChecks
{
    public static void StrictSpanProposalViewShowsTheStatusInCapitals()
    {
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
        var view = StrictSpanProposalView.ToStrictSpanProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictSpanProposalView.ToStrictSpanProposalViews(new List<StrictSpanProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictSpanProposalViewShowsTheStatusInCapitals();
    }
}
