namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayVarianceService(IRetroPayVarianceStore Store, RetroPayVarianceValidator Validator, IClock Clock, List<string> Journal, RetroPayVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay variance {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of retro pay variance workflow.</summary>
public static class RetroPayVarianceWorkflow
{
    public static RetroPayVarianceService NewRetroPayVarianceService(IRetroPayVarianceStore store, IClock clock)
    {
        return new RetroPayVarianceService(
            store,
            RetroPayVarianceRules.DefaultRetroPayVarianceValidator(),
            clock,
            new List<string>(),
            RetroPayVarianceEngine.DefaultRetroPayVarianceEngine());
    }
}
