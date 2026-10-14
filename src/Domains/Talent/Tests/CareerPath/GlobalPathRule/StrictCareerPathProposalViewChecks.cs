namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class StrictCareerPathProposalViewChecks
{
    public static void StrictCareerPathProposalViewShowsTheStatusInCapitals()
    {
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("code-178", "title-278");
        var view = StrictCareerPathProposalView.ToStrictCareerPathProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictCareerPathProposalView.ToStrictCareerPathProposalViews(new List<StrictCareerPathProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictCareerPathProposalViewShowsTheStatusInCapitals();
    }
}
