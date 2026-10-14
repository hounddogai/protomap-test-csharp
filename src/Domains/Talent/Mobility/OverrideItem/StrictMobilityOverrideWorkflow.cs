namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict mobility overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictMobilityOverrideManager(IStrictMobilityOverrideStore Store, StrictMobilityOverrideValidator Validator, IClock Clock, List<string> Journal, StrictMobilityOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict mobility override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictMobilityOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict mobility override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictMobilityOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict mobility override {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of strict mobility override workflow.</summary>
public static class StrictMobilityOverrideWorkflow
{
    public static StrictMobilityOverrideManager NewStrictMobilityOverrideManager(IStrictMobilityOverrideStore store, IClock clock, Registry flags)
    {
        return new StrictMobilityOverrideManager(
            store,
            StrictMobilityOverrideRules.DefaultStrictMobilityOverrideValidator(),
            clock,
            new List<string>(),
            StrictMobilityOverrideEngine.DefaultStrictMobilityOverrideEngine(flags));
    }
}
