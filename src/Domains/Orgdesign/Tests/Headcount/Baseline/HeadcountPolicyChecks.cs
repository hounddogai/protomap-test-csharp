namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountPolicyChecks
{
    public static void HeadcountPolicyStartsInNew()
    {
        var item = HeadcountPolicy.NewHeadcountPolicy("code-186", "label-93");
        Check.Equal(item.Status, HeadcountPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountPolicyMovesToFailed()
    {
        var item = HeadcountPolicy.NewHeadcountPolicy("code-186", "label-93");
        try
        {
            item.MoveTo(HeadcountPolicyStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountPolicyStatus.Failed, "status");
    }

    public static void HeadcountPolicyRejectsMovesFromFailed()
    {
        var item = HeadcountPolicy.NewHeadcountPolicy("code-186", "label-93");
        item.Status = HeadcountPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(HeadcountPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        HeadcountPolicyStartsInNew();
        HeadcountPolicyMovesToFailed();
        HeadcountPolicyRejectsMovesFromFailed();
    }
}
