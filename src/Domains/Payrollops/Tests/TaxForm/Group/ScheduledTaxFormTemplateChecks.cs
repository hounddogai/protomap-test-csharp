namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTaxFormTemplateChecks
{
    public static void ScheduledTaxFormTemplateStartsInCreated()
    {
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("code-19", "name-277");
        Check.Equal(item.Status, ScheduledTaxFormTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTaxFormTemplateMovesToRetired()
    {
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("code-19", "name-277");
        try
        {
            item.MoveTo(ScheduledTaxFormTemplateStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTaxFormTemplateStatus.Retired, "status");
    }

    public static void ScheduledTaxFormTemplateRejectsMovesFromRetired()
    {
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("code-19", "name-277");
        item.Status = ScheduledTaxFormTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledTaxFormTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledTaxFormTemplateStartsInCreated();
        ScheduledTaxFormTemplateMovesToRetired();
        ScheduledTaxFormTemplateRejectsMovesFromRetired();
    }
}
