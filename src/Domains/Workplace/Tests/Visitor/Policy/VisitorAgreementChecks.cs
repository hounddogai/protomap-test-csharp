namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementChecks
{
    public static void VisitorAgreementStartsInOpen()
    {
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
        Check.Equal(item.Status, VisitorAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorAgreementMovesToResolved()
    {
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
        try
        {
            item.MoveTo(VisitorAgreementStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorAgreementStatus.Resolved, "status");
    }

    public static void VisitorAgreementRejectsMovesFromArchived()
    {
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
        item.Status = VisitorAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(VisitorAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        VisitorAgreementStartsInOpen();
        VisitorAgreementMovesToResolved();
        VisitorAgreementRejectsMovesFromArchived();
    }
}
