namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryDeductionBudgetChecks
{
    public static void PrimaryDeductionBudgetStartsInCreated()
    {
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("code-869", "name-738");
        Check.Equal(item.Status, PrimaryDeductionBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryDeductionBudgetMovesToValidated()
    {
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("code-869", "name-738");
        try
        {
            item.MoveTo(PrimaryDeductionBudgetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryDeductionBudgetStatus.Validated, "status");
    }

    public static void PrimaryDeductionBudgetRejectsMovesFromRetired()
    {
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("code-869", "name-738");
        item.Status = PrimaryDeductionBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryDeductionBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryDeductionBudgetStartsInCreated();
        PrimaryDeductionBudgetMovesToValidated();
        PrimaryDeductionBudgetRejectsMovesFromRetired();
    }
}
