namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayDigestManager(IRetroPayDigestStore Store, RetroPayDigestValidator Validator, IClock Clock, List<string> Journal, RetroPayDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayDigestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayDigestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay digest {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of retro pay digest workflow.</summary>
public static class RetroPayDigestWorkflow
{
    public static RetroPayDigestManager NewRetroPayDigestManager(IRetroPayDigestStore store, IClock clock)
    {
        return new RetroPayDigestManager(
            store,
            RetroPayDigestRules.DefaultRetroPayDigestValidator(),
            clock,
            new List<string>(),
            RetroPayDigestEngine.DefaultRetroPayDigestEngine());
    }
}
