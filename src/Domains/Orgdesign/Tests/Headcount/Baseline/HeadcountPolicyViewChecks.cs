namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountPolicyViewChecks
{
    public static void HeadcountPolicyViewShowsTheStatusInCapitals()
    {
        var item = HeadcountPolicy.NewHeadcountPolicy("code-186", "label-93");
        var view = HeadcountPolicyView.ToHeadcountPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountPolicyView.ToHeadcountPolicyViews(new List<HeadcountPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountPolicyViewShowsTheStatusInCapitals();
    }
}
