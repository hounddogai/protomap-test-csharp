namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunMetricService(IPayRunMetricStore Store, PayRunMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of pay run metric workflow.</summary>
public static class PayRunMetricWorkflow
{
    public static PayRunMetricService NewPayRunMetricService(IPayRunMetricStore store, IClock clock)
    {
        return new PayRunMetricService(
            store,
            PayRunMetricRules.DefaultPayRunMetricValidator(),
            clock,
            new List<string>());
    }
}
