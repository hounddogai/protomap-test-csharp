namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientPolicyAckTemplateChecks
{
    public static void LenientPolicyAckTemplateStartsInIdle()
    {
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("code-40", "title-134");
        Check.Equal(item.Status, LenientPolicyAckTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPolicyAckTemplateMovesToReady()
    {
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("code-40", "title-134");
        try
        {
            item.MoveTo(LenientPolicyAckTemplateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPolicyAckTemplateStatus.Ready, "status");
    }

    public static void LenientPolicyAckTemplateRejectsMovesFromStopped()
    {
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("code-40", "title-134");
        item.Status = LenientPolicyAckTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientPolicyAckTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientPolicyAckTemplateStartsInIdle();
        LenientPolicyAckTemplateMovesToReady();
        LenientPolicyAckTemplateRejectsMovesFromStopped();
    }
}
