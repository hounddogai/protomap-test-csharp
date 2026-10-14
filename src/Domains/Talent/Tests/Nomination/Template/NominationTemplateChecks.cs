namespace Acme.Workforce.Talent.Tests.Nomination.Template;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class NominationTemplateChecks
{
    public static void NominationTemplateStartsInRequested()
    {
        var item = NominationTemplate.NewNominationTemplate("code-636", "name-461");
        Check.Equal(item.Status, NominationTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationTemplateMovesToGranted()
    {
        var item = NominationTemplate.NewNominationTemplate("code-636", "name-461");
        try
        {
            item.MoveTo(NominationTemplateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationTemplateStatus.Granted, "status");
    }

    public static void NominationTemplateRejectsMovesFromExpired()
    {
        var item = NominationTemplate.NewNominationTemplate("code-636", "name-461");
        item.Status = NominationTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(NominationTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        NominationTemplateStartsInRequested();
        NominationTemplateMovesToGranted();
        NominationTemplateRejectsMovesFromExpired();
    }
}
