namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceCheckpointViewChecks
{
    public static void LenientBalanceCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("code-728", "title-794");
        var view = LenientBalanceCheckpointView.ToLenientBalanceCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBalanceCheckpointView.ToLenientBalanceCheckpointViews(new List<LenientBalanceCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBalanceCheckpointViewShowsTheStatusInCapitals();
    }
}
