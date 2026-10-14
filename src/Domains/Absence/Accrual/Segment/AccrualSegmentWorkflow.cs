namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualSegmentCoordinator(IAccrualSegmentStore Store, AccrualSegmentValidator Validator, IClock Clock, List<string> Journal, AccrualSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualSegmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualSegmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual segment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of accrual segment workflow.</summary>
public static class AccrualSegmentWorkflow
{
    public static AccrualSegmentCoordinator NewAccrualSegmentCoordinator(IAccrualSegmentStore store, IClock clock)
    {
        return new AccrualSegmentCoordinator(
            store,
            AccrualSegmentRules.DefaultAccrualSegmentValidator(),
            clock,
            new List<string>(),
            AccrualSegmentEngine.DefaultAccrualSegmentEngine());
    }
}
