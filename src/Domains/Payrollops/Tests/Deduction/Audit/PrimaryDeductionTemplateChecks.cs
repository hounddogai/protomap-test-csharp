namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryDeductionTemplateChecks
{
    public static void PrimaryDeductionTemplateStartsInDraft()
    {
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("code-168", "title-306");
        Check.Equal(item.Status, PrimaryDeductionTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryDeductionTemplateMovesToClosed()
    {
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("code-168", "title-306");
        try
        {
            item.MoveTo(PrimaryDeductionTemplateStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryDeductionTemplateStatus.Closed, "status");
    }

    public static void PrimaryDeductionTemplateRejectsMovesFromClosed()
    {
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("code-168", "title-306");
        item.Status = PrimaryDeductionTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryDeductionTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryDeductionTemplateStartsInDraft();
        PrimaryDeductionTemplateMovesToClosed();
        PrimaryDeductionTemplateRejectsMovesFromClosed();
    }
}
