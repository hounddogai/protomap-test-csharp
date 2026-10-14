namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyEntryWorkflow(IPolicyEntryStore Store, PolicyEntryValidator Validator, IClock Clock, List<string> Journal, PolicyEntryEngine Engine)
{
    public static PolicyEntryWorkflow NewPolicyEntryWorkflow(IPolicyEntryStore store, IClock clock)
    {
        return new PolicyEntryWorkflow(
            store,
            PolicyEntryRules.DefaultPolicyEntryValidator(),
            clock,
            new List<string>(),
            PolicyEntryEngine.DefaultPolicyEntryEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy entry {code} not found");
        }
        var result = Engine.Run(item);
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
