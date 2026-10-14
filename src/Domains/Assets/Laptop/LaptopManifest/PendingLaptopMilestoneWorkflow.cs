namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending laptop milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingLaptopMilestoneService(IPendingLaptopMilestoneStore Store, PendingLaptopMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending laptop milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingLaptopMilestoneStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending laptop milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingLaptopMilestoneStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of pending laptop milestone workflow.</summary>
public static class PendingLaptopMilestoneWorkflow
{
    public static PendingLaptopMilestoneService NewPendingLaptopMilestoneService(IPendingLaptopMilestoneStore store, IClock clock)
    {
        return new PendingLaptopMilestoneService(
            store,
            PendingLaptopMilestoneRules.DefaultPendingLaptopMilestoneValidator(),
            clock,
            new List<string>());
    }
}
