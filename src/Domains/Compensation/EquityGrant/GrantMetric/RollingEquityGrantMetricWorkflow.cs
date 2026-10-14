namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling equity grant metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingEquityGrantMetricWorkflow(IRollingEquityGrantMetricStore Store, RollingEquityGrantMetricValidator Validator, IClock Clock, List<string> Journal, RollingEquityGrantMetricEngine Engine)
{
    public static RollingEquityGrantMetricWorkflow NewRollingEquityGrantMetricWorkflow(IRollingEquityGrantMetricStore store, IClock clock)
    {
        return new RollingEquityGrantMetricWorkflow(
            store,
            RollingEquityGrantMetricRules.DefaultRollingEquityGrantMetricValidator(),
            clock,
            new List<string>(),
            RollingEquityGrantMetricEngine.DefaultRollingEquityGrantMetricEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling equity grant metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingEquityGrantMetricStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling equity grant metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingEquityGrantMetricStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling equity grant metric {code} not found");
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
            if (item.CreatedAt > cutoff)
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
