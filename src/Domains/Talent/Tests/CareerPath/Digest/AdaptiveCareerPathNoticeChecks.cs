namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AdaptiveCareerPathNoticeChecks
{
    public static void AdaptiveCareerPathNoticeStartsInNew()
    {
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("code-792", "label-908");
        Check.Equal(item.Status, AdaptiveCareerPathNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCareerPathNoticeMovesToDone()
    {
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("code-792", "label-908");
        try
        {
            item.MoveTo(AdaptiveCareerPathNoticeStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCareerPathNoticeStatus.Done, "status");
    }

    public static void AdaptiveCareerPathNoticeRejectsMovesFromFailed()
    {
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("code-792", "label-908");
        item.Status = AdaptiveCareerPathNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveCareerPathNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathNoticeStartsInNew();
        AdaptiveCareerPathNoticeMovesToDone();
        AdaptiveCareerPathNoticeRejectsMovesFromFailed();
    }
}
