namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayClaimManager(IRetroPayClaimStore Store, RetroPayClaimValidator Validator, IClock Clock, List<string> Journal, RetroPayClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay claim {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of retro pay claim workflow.</summary>
public static class RetroPayClaimWorkflow
{
    public static RetroPayClaimManager NewRetroPayClaimManager(IRetroPayClaimStore store, IClock clock)
    {
        return new RetroPayClaimManager(
            store,
            RetroPayClaimRules.DefaultRetroPayClaimValidator(),
            clock,
            new List<string>(),
            RetroPayClaimEngine.DefaultRetroPayClaimEngine());
    }
}
