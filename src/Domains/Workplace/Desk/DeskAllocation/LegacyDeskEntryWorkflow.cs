namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy desk entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyDeskEntryManager(ILegacyDeskEntryStore Store, LegacyDeskEntryValidator Validator, IClock Clock, List<string> Journal, LegacyDeskEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy desk entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyDeskEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy desk entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyDeskEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy desk entry {code} not found");
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

/// <summary>Functions of legacy desk entry workflow.</summary>
public static class LegacyDeskEntryWorkflow
{
    public static LegacyDeskEntryManager NewLegacyDeskEntryManager(ILegacyDeskEntryStore store, IClock clock)
    {
        return new LegacyDeskEntryManager(
            store,
            LegacyDeskEntryRules.DefaultLegacyDeskEntryValidator(),
            clock,
            new List<string>(),
            LegacyDeskEntryEngine.DefaultLegacyDeskEntryEngine());
    }
}
