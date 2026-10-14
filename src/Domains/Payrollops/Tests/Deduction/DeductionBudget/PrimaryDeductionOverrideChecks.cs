namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class PrimaryDeductionOverrideChecks
{
    public static void PrimaryDeductionOverrideStartsInCreated()
    {
        var item = PrimaryDeductionOverride.NewPrimaryDeductionOverride("code-408", "name-175");
        Check.Equal(item.Status, PrimaryDeductionOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryDeductionOverrideMovesToRetired()
    {
        var item = PrimaryDeductionOverride.NewPrimaryDeductionOverride("code-408", "name-175");
        try
        {
            item.MoveTo(PrimaryDeductionOverrideStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryDeductionOverrideStatus.Retired, "status");
    }

    public static void PrimaryDeductionOverrideRejectsMovesFromRetired()
    {
        var item = PrimaryDeductionOverride.NewPrimaryDeductionOverride("code-408", "name-175");
        item.Status = PrimaryDeductionOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryDeductionOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryDeductionOverrideStartsInCreated();
        PrimaryDeductionOverrideMovesToRetired();
        PrimaryDeductionOverrideRejectsMovesFromRetired();
    }
}
