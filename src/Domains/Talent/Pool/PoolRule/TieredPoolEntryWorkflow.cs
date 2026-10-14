namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered pool entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPoolEntryDesk(ITieredPoolEntryStore Store, TieredPoolEntryValidator Validator, IClock Clock, List<string> Journal, TieredPoolEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pool entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPoolEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pool entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPoolEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pool entry {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tiered pool entry workflow.</summary>
public static class TieredPoolEntryWorkflow
{
    public static TieredPoolEntryDesk NewTieredPoolEntryDesk(ITieredPoolEntryStore store, IClock clock, Registry flags)
    {
        return new TieredPoolEntryDesk(
            store,
            TieredPoolEntryRules.DefaultTieredPoolEntryValidator(),
            clock,
            new List<string>(),
            TieredPoolEntryEngine.DefaultTieredPoolEntryEngine(flags));
    }
}
