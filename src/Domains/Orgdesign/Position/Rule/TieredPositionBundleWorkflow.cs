namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered position bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPositionBundleDesk(ITieredPositionBundleStore Store, TieredPositionBundleValidator Validator, IClock Clock, List<string> Journal, TieredPositionBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPositionBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPositionBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position bundle {code} not found");
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

/// <summary>Functions of tiered position bundle workflow.</summary>
public static class TieredPositionBundleWorkflow
{
    public static TieredPositionBundleDesk NewTieredPositionBundleDesk(ITieredPositionBundleStore store, IClock clock)
    {
        return new TieredPositionBundleDesk(
            store,
            TieredPositionBundleRules.DefaultTieredPositionBundleValidator(),
            clock,
            new List<string>(),
            TieredPositionBundleEngine.DefaultTieredPositionBundleEngine());
    }
}
