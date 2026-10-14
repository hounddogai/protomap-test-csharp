namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback statement of work segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackStatementOfWorkSegmentCoordinator(IFallbackStatementOfWorkSegmentStore Store, FallbackStatementOfWorkSegmentValidator Validator, IClock Clock, List<string> Journal, FallbackStatementOfWorkSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback statement of work segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackStatementOfWorkSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback statement of work segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackStatementOfWorkSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback statement of work segment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of fallback statement of work segment workflow.</summary>
public static class FallbackStatementOfWorkSegmentWorkflow
{
    public static FallbackStatementOfWorkSegmentCoordinator NewFallbackStatementOfWorkSegmentCoordinator(IFallbackStatementOfWorkSegmentStore store, IClock clock)
    {
        return new FallbackStatementOfWorkSegmentCoordinator(
            store,
            FallbackStatementOfWorkSegmentRules.DefaultFallbackStatementOfWorkSegmentValidator(),
            clock,
            new List<string>(),
            FallbackStatementOfWorkSegmentEngine.DefaultFallbackStatementOfWorkSegmentEngine());
    }
}
