namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshCheckpointDesk(IRefreshCheckpointStore Store, RefreshCheckpointValidator Validator, IClock Clock, List<string> Journal, RefreshCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of refresh checkpoint workflow.</summary>
public static class RefreshCheckpointWorkflow
{
    public static RefreshCheckpointDesk NewRefreshCheckpointDesk(IRefreshCheckpointStore store, IClock clock)
    {
        return new RefreshCheckpointDesk(
            store,
            RefreshCheckpointRules.DefaultRefreshCheckpointValidator(),
            clock,
            new List<string>(),
            RefreshCheckpointEngine.DefaultRefreshCheckpointEngine());
    }
}
