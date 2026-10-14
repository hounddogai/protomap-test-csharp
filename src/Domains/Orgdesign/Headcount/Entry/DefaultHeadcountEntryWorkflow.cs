namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default headcount entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultHeadcountEntryManager(IDefaultHeadcountEntryStore Store, DefaultHeadcountEntryValidatorExtra Validator, IClock Clock, List<string> Journal, DefaultHeadcountEntryEngineExtra Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultHeadcountEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultHeadcountEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount entry {code} not found");
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

/// <summary>Functions of default headcount entry workflow.</summary>
public static class DefaultHeadcountEntryWorkflow
{
    public static DefaultHeadcountEntryManager NewDefaultHeadcountEntryManager(IDefaultHeadcountEntryStore store, IClock clock, Registry flags)
    {
        return new DefaultHeadcountEntryManager(
            store,
            DefaultHeadcountEntryRules.DefaultDefaultHeadcountEntryValidator(),
            clock,
            new List<string>(),
            DefaultHeadcountEntryEngine.DefaultDefaultHeadcountEntryEngine(flags));
    }
}
