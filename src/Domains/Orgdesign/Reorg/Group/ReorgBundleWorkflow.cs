namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves reorg bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgBundleManager(IReorgBundleStore Store, ReorgBundleValidator Validator, IClock Clock, List<string> Journal, ReorgBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg bundle {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of reorg bundle workflow.</summary>
public static class ReorgBundleWorkflow
{
    public static ReorgBundleManager NewReorgBundleManager(IReorgBundleStore store, IClock clock, Registry flags)
    {
        return new ReorgBundleManager(
            store,
            ReorgBundleRules.DefaultReorgBundleValidator(),
            clock,
            new List<string>(),
            ReorgBundleEngine.DefaultReorgBundleEngine(flags));
    }
}
