namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class RegionalPayRunTemplateChecks
{
    public static void RegionalPayRunTemplateStartsInOpen()
    {
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("code-179", "label-737");
        Check.Equal(item.Status, RegionalPayRunTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayRunTemplateMovesToResolved()
    {
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("code-179", "label-737");
        try
        {
            item.MoveTo(RegionalPayRunTemplateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayRunTemplateStatus.Resolved, "status");
    }

    public static void RegionalPayRunTemplateRejectsMovesFromArchived()
    {
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("code-179", "label-737");
        item.Status = RegionalPayRunTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalPayRunTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalPayRunTemplateStartsInOpen();
        RegionalPayRunTemplateMovesToResolved();
        RegionalPayRunTemplateRejectsMovesFromArchived();
    }
}
