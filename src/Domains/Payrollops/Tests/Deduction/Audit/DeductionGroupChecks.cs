namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionGroupChecks
{
    public static void DeductionGroupStartsInCreated()
    {
        var item = DeductionGroup.NewDeductionGroup("code-742", "title-302");
        Check.Equal(item.Status, DeductionGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionGroupMovesToPublished()
    {
        var item = DeductionGroup.NewDeductionGroup("code-742", "title-302");
        try
        {
            item.MoveTo(DeductionGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionGroupStatus.Published, "status");
    }

    public static void DeductionGroupRejectsMovesFromRetired()
    {
        var item = DeductionGroup.NewDeductionGroup("code-742", "title-302");
        item.Status = DeductionGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(DeductionGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeductionGroupStartsInCreated();
        DeductionGroupMovesToPublished();
        DeductionGroupRejectsMovesFromRetired();
    }
}
