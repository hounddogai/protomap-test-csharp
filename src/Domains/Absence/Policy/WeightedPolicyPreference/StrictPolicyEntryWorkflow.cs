namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict policy entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPolicyEntryDesk(IStrictPolicyEntryStore Store, StrictPolicyEntryValidator Validator, IClock Clock, List<string> Journal, StrictPolicyEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPolicyEntryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPolicyEntryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy entry {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of strict policy entry workflow.</summary>
public static class StrictPolicyEntryWorkflow
{
    public static StrictPolicyEntryDesk NewStrictPolicyEntryDesk(IStrictPolicyEntryStore store, IClock clock, Registry flags)
    {
        return new StrictPolicyEntryDesk(
            store,
            StrictPolicyEntryRules.DefaultStrictPolicyEntryValidator(),
            clock,
            new List<string>(),
            StrictPolicyEntryEngine.DefaultStrictPolicyEntryEngine(flags));
    }
}
