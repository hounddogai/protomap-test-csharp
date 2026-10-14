namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending assignment tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingAssignmentTierWorkflow(IPendingAssignmentTierStore Store, PendingAssignmentTierValidator Validator, IClock Clock, List<string> Journal)
{
    public static PendingAssignmentTierWorkflow NewPendingAssignmentTierWorkflow(IPendingAssignmentTierStore store, IClock clock)
    {
        return new PendingAssignmentTierWorkflow(
            store,
            PendingAssignmentTierRules.DefaultPendingAssignmentTierValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending assignment tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAssignmentTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending assignment tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAssignmentTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
