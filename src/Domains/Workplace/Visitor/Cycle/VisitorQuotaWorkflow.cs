namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorQuotaService(IVisitorQuotaStore Store, VisitorQuotaValidator Validator, IClock Clock, List<string> Journal, VisitorQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor quota {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of visitor quota workflow.</summary>
public static class VisitorQuotaWorkflow
{
    public static VisitorQuotaService NewVisitorQuotaService(IVisitorQuotaStore store, IClock clock)
    {
        return new VisitorQuotaService(
            store,
            VisitorQuotaRules.DefaultVisitorQuotaValidator(),
            clock,
            new List<string>(),
            VisitorQuotaEngine.DefaultVisitorQuotaEngine());
    }
}
