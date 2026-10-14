namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryDeductionTemplateViewChecks
{
    public static void PrimaryDeductionTemplateViewShowsTheStatusInCapitals()
    {
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("code-168", "title-306");
        var view = PrimaryDeductionTemplateView.ToPrimaryDeductionTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryDeductionTemplateView.ToPrimaryDeductionTemplateViews(new List<PrimaryDeductionTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryDeductionTemplateViewShowsTheStatusInCapitals();
    }
}
