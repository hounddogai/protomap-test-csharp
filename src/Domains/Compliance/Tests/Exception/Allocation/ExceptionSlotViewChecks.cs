namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionSlotViewChecks
{
    public static void ExceptionSlotViewShowsTheStatusInCapitals()
    {
        var item = ExceptionSlot.NewExceptionSlot("code-715", "name-79");
        var view = ExceptionSlotView.ToExceptionSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionSlotView.ToExceptionSlotViews(new List<ExceptionSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionSlotViewShowsTheStatusInCapitals();
    }
}
