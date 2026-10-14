namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual license seat slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualLicenseSeatSlotWorkflow(IManualLicenseSeatSlotStore Store, ManualLicenseSeatSlotValidator Validator, IClock Clock, List<string> Journal)
{
    public static ManualLicenseSeatSlotWorkflow NewManualLicenseSeatSlotWorkflow(IManualLicenseSeatSlotStore store, IClock clock)
    {
        return new ManualLicenseSeatSlotWorkflow(
            store,
            ManualLicenseSeatSlotRules.DefaultManualLicenseSeatSlotValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual license seat slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualLicenseSeatSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual license seat slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualLicenseSeatSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}
