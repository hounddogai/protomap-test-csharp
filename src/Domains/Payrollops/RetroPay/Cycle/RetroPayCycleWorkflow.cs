namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayCycleService(IRetroPayCycleStore Store, RetroPayCycleValidator Validator, IClock Clock, List<string> Journal, RetroPayCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayCycleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayCycleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay cycle {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of retro pay cycle workflow.</summary>
public static class RetroPayCycleWorkflow
{
    public static RetroPayCycleService NewRetroPayCycleService(IRetroPayCycleStore store, IClock clock)
    {
        return new RetroPayCycleService(
            store,
            RetroPayCycleRules.DefaultRetroPayCycleValidator(),
            clock,
            new List<string>(),
            RetroPayCycleEngine.DefaultRetroPayCycleEngine());
    }
}
