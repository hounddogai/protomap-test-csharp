namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class DefaultWarrantyCycleChecks
{
    public static void DefaultWarrantyCycleStartsInPlanned()
    {
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("code-226", "name-125");
        Check.Equal(item.Status, DefaultWarrantyCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultWarrantyCycleMovesToCancelled()
    {
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("code-226", "name-125");
        try
        {
            item.MoveTo(DefaultWarrantyCycleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultWarrantyCycleStatus.Cancelled, "status");
    }

    public static void DefaultWarrantyCycleRejectsMovesFromCancelled()
    {
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("code-226", "name-125");
        item.Status = DefaultWarrantyCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultWarrantyCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultWarrantyCycleStartsInPlanned();
        DefaultWarrantyCycleMovesToCancelled();
        DefaultWarrantyCycleRejectsMovesFromCancelled();
    }
}
