namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunBaselineCoordinator(IPayRunBaselineStore Store, PayRunBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunBaselineStatus.Retired);
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

/// <summary>Functions of pay run baseline workflow.</summary>
public static class PayRunBaselineWorkflow
{
    public static PayRunBaselineCoordinator NewPayRunBaselineCoordinator(IPayRunBaselineStore store, IClock clock)
    {
        return new PayRunBaselineCoordinator(
            store,
            PayRunBaselineRules.DefaultPayRunBaselineValidator(),
            clock,
            new List<string>());
    }
}
