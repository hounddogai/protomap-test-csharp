namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict policy preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPolicyPreferenceManager(IStrictPolicyPreferenceStore Store, StrictPolicyPreferenceValidator Validator, IClock Clock, List<string> Journal, StrictPolicyPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPolicyPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPolicyPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy preference {code} not found");
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

/// <summary>Functions of strict policy preference workflow.</summary>
public static class StrictPolicyPreferenceWorkflow
{
    public static StrictPolicyPreferenceManager NewStrictPolicyPreferenceManager(IStrictPolicyPreferenceStore store, IClock clock)
    {
        return new StrictPolicyPreferenceManager(
            store,
            StrictPolicyPreferenceRules.DefaultStrictPolicyPreferenceValidator(),
            clock,
            new List<string>(),
            StrictPolicyPreferenceEngine.DefaultStrictPolicyPreferenceEngine());
    }
}
