namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default span baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultSpanBaselineService(IDefaultSpanBaselineStore Store, DefaultSpanBaselineValidator Validator, IClock Clock, List<string> Journal, DefaultSpanBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSpanBaselineStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSpanBaselineStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span baseline {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of default span baseline workflow.</summary>
public static class DefaultSpanBaselineWorkflow
{
    public static DefaultSpanBaselineService NewDefaultSpanBaselineService(IDefaultSpanBaselineStore store, IClock clock, Registry flags)
    {
        return new DefaultSpanBaselineService(
            store,
            DefaultSpanBaselineRules.DefaultDefaultSpanBaselineValidator(),
            clock,
            new List<string>(),
            DefaultSpanBaselineEngine.DefaultDefaultSpanBaselineEngine(flags));
    }
}
