namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling rate bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRateBundleService(IRollingRateBundleStore Store, RollingRateBundleValidator Validator, IClock Clock, List<string> Journal, RollingRateBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling rate bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRateBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling rate bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRateBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling rate bundle {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of rolling rate bundle workflow.</summary>
public static class RollingRateBundleWorkflow
{
    public static RollingRateBundleService NewRollingRateBundleService(IRollingRateBundleStore store, IClock clock, Registry flags)
    {
        return new RollingRateBundleService(
            store,
            RollingRateBundleRules.DefaultRollingRateBundleValidator(),
            clock,
            new List<string>(),
            RollingRateBundleEngine.DefaultRollingRateBundleEngine(flags));
    }
}
