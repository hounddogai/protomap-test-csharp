namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyProfileDesk(IPolicyProfileStore Store, PolicyProfileValidator Validator, IClock Clock, List<string> Journal, PolicyProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy profile {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of policy profile workflow.</summary>
public static class PolicyProfileWorkflow
{
    public static PolicyProfileDesk NewPolicyProfileDesk(IPolicyProfileStore store, IClock clock)
    {
        return new PolicyProfileDesk(
            store,
            PolicyProfileRules.DefaultPolicyProfileValidator(),
            clock,
            new List<string>(),
            PolicyProfileEngine.DefaultPolicyProfileEngine());
    }
}
