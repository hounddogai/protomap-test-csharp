namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointViewChecks
{
    public static void ExceptionCheckpointViewShowsTheStatusInCapitals()
    {
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
        var view = ExceptionCheckpointView.ToExceptionCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionCheckpointView.ToExceptionCheckpointViews(new List<ExceptionCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionCheckpointViewShowsTheStatusInCapitals();
    }
}
