namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped rate metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedRateMetricWorkflow(ICappedRateMetricStore Store, CappedRateMetricValidator Validator, IClock Clock, List<string> Journal, CappedRateMetricEngine Engine)
{
    public static CappedRateMetricWorkflow NewCappedRateMetricWorkflow(ICappedRateMetricStore store, IClock clock, Registry flags)
    {
        return new CappedRateMetricWorkflow(
            store,
            CappedRateMetricRules.DefaultCappedRateMetricValidator(),
            clock,
            new List<string>(),
            CappedRateMetricEngine.DefaultCappedRateMetricEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped rate metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedRateMetricStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped rate metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedRateMetricStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped rate metric {code} not found");
        }
        var result = Engine.Run(item);
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
