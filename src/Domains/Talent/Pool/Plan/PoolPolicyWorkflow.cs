namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolPolicyService(IPoolPolicyStore Store, PoolPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pool policy workflow.</summary>
public static class PoolPolicyWorkflow
{
    public static PoolPolicyService NewPoolPolicyService(IPoolPolicyStore store, IClock clock)
    {
        return new PoolPolicyService(store, PoolPolicyRules.DefaultPoolPolicyValidator(), clock, new List<string>());
    }
}
