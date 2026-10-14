namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountCheckpointCoordinator(IHeadcountCheckpointStore Store, HeadcountCheckpointValidator Validator, IClock Clock, List<string> Journal, HeadcountCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount checkpoint {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of headcount checkpoint workflow.</summary>
public static class HeadcountCheckpointWorkflow
{
    public static HeadcountCheckpointCoordinator NewHeadcountCheckpointCoordinator(IHeadcountCheckpointStore store, IClock clock)
    {
        return new HeadcountCheckpointCoordinator(
            store,
            HeadcountCheckpointRules.DefaultHeadcountCheckpointValidator(),
            clock,
            new List<string>(),
            HeadcountCheckpointEngine.DefaultHeadcountCheckpointEngine());
    }
}
