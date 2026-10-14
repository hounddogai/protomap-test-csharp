namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy license seat cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyLicenseSeatCycleDesk(ILegacyLicenseSeatCycleStore Store, LegacyLicenseSeatCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy license seat cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyLicenseSeatCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy license seat cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyLicenseSeatCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of legacy license seat cycle workflow.</summary>
public static class LegacyLicenseSeatCycleWorkflow
{
    public static LegacyLicenseSeatCycleDesk NewLegacyLicenseSeatCycleDesk(ILegacyLicenseSeatCycleStore store, IClock clock)
    {
        return new LegacyLicenseSeatCycleDesk(
            store,
            LegacyLicenseSeatCycleRules.DefaultLegacyLicenseSeatCycleValidator(),
            clock,
            new List<string>());
    }
}
