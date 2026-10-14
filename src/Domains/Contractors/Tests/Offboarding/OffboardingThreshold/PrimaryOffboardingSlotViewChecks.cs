namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class PrimaryOffboardingSlotViewChecks
{
    public static void PrimaryOffboardingSlotViewShowsTheStatusInCapitals()
    {
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("code-416", "name-182");
        var view = PrimaryOffboardingSlotView.ToPrimaryOffboardingSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryOffboardingSlotView.ToPrimaryOffboardingSlotViews(new List<PrimaryOffboardingSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryOffboardingSlotViewShowsTheStatusInCapitals();
    }
}
