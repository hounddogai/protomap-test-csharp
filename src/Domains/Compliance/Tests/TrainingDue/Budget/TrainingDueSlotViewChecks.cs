namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TrainingDueSlotViewChecks
{
    public static void TrainingDueSlotViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueSlot.NewTrainingDueSlot("code-95", "label-241");
        var view = TrainingDueSlotView.ToTrainingDueSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueSlotView.ToTrainingDueSlotViews(new List<TrainingDueSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueSlotViewShowsTheStatusInCapitals();
    }
}
