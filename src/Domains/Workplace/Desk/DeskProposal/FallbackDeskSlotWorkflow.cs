namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback desk slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackDeskSlotDesk(IFallbackDeskSlotStore Store, FallbackDeskSlotValidator Validator, IClock Clock, List<string> Journal, FallbackDeskSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackDeskSlotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackDeskSlotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk slot {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback desk slot workflow.</summary>
public static class FallbackDeskSlotWorkflow
{
    public static FallbackDeskSlotDesk NewFallbackDeskSlotDesk(IFallbackDeskSlotStore store, IClock clock)
    {
        return new FallbackDeskSlotDesk(
            store,
            FallbackDeskSlotRules.DefaultFallbackDeskSlotValidator(),
            clock,
            new List<string>(),
            FallbackDeskSlotEngine.DefaultFallbackDeskSlotEngine());
    }
}
