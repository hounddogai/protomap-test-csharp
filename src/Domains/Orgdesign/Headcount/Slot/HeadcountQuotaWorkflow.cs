namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountQuotaWorkflow(IHeadcountQuotaStore Store, HeadcountQuotaValidator Validator, IClock Clock, List<string> Journal, HeadcountQuotaEngine Engine)
{
    public static HeadcountQuotaWorkflow NewHeadcountQuotaWorkflow(IHeadcountQuotaStore store, IClock clock)
    {
        return new HeadcountQuotaWorkflow(
            store,
            HeadcountQuotaRules.DefaultHeadcountQuotaValidator(),
            clock,
            new List<string>(),
            HeadcountQuotaEngine.DefaultHeadcountQuotaEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount quota {code} not found");
        }
        var result = Engine.Compute(item);
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
