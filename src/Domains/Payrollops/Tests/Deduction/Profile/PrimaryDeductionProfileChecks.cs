namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryDeductionProfileChecks
{
    public static void PrimaryDeductionProfileStartsInPlanned()
    {
        var item = PrimaryDeductionProfile.NewPrimaryDeductionProfile("code-810", "title-366");
        Check.Equal(item.Status, PrimaryDeductionProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryDeductionProfileMovesToCompleted()
    {
        var item = PrimaryDeductionProfile.NewPrimaryDeductionProfile("code-810", "title-366");
        try
        {
            item.MoveTo(PrimaryDeductionProfileStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryDeductionProfileStatus.Completed, "status");
    }

    public static void PrimaryDeductionProfileRejectsMovesFromCancelled()
    {
        var item = PrimaryDeductionProfile.NewPrimaryDeductionProfile("code-810", "title-366");
        item.Status = PrimaryDeductionProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryDeductionProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryDeductionProfileStartsInPlanned();
        PrimaryDeductionProfileMovesToCompleted();
        PrimaryDeductionProfileRejectsMovesFromCancelled();
    }
}
