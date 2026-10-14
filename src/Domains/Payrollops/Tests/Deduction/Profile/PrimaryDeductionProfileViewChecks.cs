namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryDeductionProfileViewChecks
{
    public static void PrimaryDeductionProfileViewShowsTheStatusInCapitals()
    {
        var item = PrimaryDeductionProfile.NewPrimaryDeductionProfile("code-810", "title-366");
        var view = PrimaryDeductionProfileView.ToPrimaryDeductionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryDeductionProfileView.ToPrimaryDeductionProfileViews(new List<PrimaryDeductionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryDeductionProfileViewShowsTheStatusInCapitals();
    }
}
