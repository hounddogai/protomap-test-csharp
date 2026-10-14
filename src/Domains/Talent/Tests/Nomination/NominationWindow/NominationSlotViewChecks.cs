namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotViewChecks
{
    public static void NominationSlotViewShowsTheStatusInCapitals()
    {
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
        var view = NominationSlotView.ToNominationSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationSlotView.ToNominationSlotViews(new List<NominationSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationSlotViewShowsTheStatusInCapitals();
    }
}
