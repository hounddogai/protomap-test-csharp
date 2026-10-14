namespace Acme.Workforce.Compliance.Tests.TrainingDue.Tier;

using Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientTrainingDueTierChecks
{
    public static void LenientTrainingDueTierStartsInIdle()
    {
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("code-213", "title-486");
        Check.Equal(item.Status, LenientTrainingDueTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTrainingDueTierMovesToWarming()
    {
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("code-213", "title-486");
        try
        {
            item.MoveTo(LenientTrainingDueTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTrainingDueTierStatus.Warming, "status");
    }

    public static void LenientTrainingDueTierRejectsMovesFromStopped()
    {
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("code-213", "title-486");
        item.Status = LenientTrainingDueTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientTrainingDueTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientTrainingDueTierStartsInIdle();
        LenientTrainingDueTierMovesToWarming();
        LenientTrainingDueTierRejectsMovesFromStopped();
    }
}
