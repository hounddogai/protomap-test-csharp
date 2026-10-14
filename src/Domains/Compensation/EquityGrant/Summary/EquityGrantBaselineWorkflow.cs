namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves equity grant baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantBaselineManager(IEquityGrantBaselineStore Store, EquityGrantBaselineValidator Validator, IClock Clock, List<string> Journal, EquityGrantBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantBaselineStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant baseline {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of equity grant baseline workflow.</summary>
public static class EquityGrantBaselineWorkflow
{
    public static EquityGrantBaselineManager NewEquityGrantBaselineManager(IEquityGrantBaselineStore store, IClock clock, Registry flags)
    {
        return new EquityGrantBaselineManager(
            store,
            EquityGrantBaselineRules.DefaultEquityGrantBaselineValidator(),
            clock,
            new List<string>(),
            EquityGrantBaselineEngine.DefaultEquityGrantBaselineEngine(flags));
    }
}
