namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeChecks
{
    public static void AccrualNoticeStartsInCreated()
    {
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
        Check.Equal(item.Status, AccrualNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualNoticeMovesToValidated()
    {
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
        try
        {
            item.MoveTo(AccrualNoticeStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualNoticeStatus.Validated, "status");
    }

    public static void AccrualNoticeRejectsMovesFromRetired()
    {
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
        item.Status = AccrualNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(AccrualNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccrualNoticeStartsInCreated();
        AccrualNoticeMovesToValidated();
        AccrualNoticeRejectsMovesFromRetired();
    }
}
