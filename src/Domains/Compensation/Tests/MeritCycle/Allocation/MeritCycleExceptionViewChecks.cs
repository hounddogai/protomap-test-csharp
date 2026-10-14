namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleExceptionViewChecks
{
    public static void MeritCycleExceptionViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleException.NewMeritCycleException("code-984", "title-950");
        var view = MeritCycleExceptionView.ToMeritCycleExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleExceptionView.ToMeritCycleExceptionViews(new List<MeritCycleException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleExceptionViewShowsTheStatusInCapitals();
    }
}
