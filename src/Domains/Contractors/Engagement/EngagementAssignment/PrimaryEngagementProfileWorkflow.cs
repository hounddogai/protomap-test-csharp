namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary engagement profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryEngagementProfileDesk(IPrimaryEngagementProfileStore Store, PrimaryEngagementProfileValidator Validator, IClock Clock, List<string> Journal, PrimaryEngagementProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary engagement profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryEngagementProfileStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary engagement profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryEngagementProfileStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary engagement profile {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of primary engagement profile workflow.</summary>
public static class PrimaryEngagementProfileWorkflow
{
    public static PrimaryEngagementProfileDesk NewPrimaryEngagementProfileDesk(IPrimaryEngagementProfileStore store, IClock clock)
    {
        return new PrimaryEngagementProfileDesk(
            store,
            PrimaryEngagementProfileRules.DefaultPrimaryEngagementProfileValidator(),
            clock,
            new List<string>(),
            PrimaryEngagementProfileEngine.DefaultPrimaryEngagementProfileEngine());
    }
}
