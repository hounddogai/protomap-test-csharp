namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorEstimateCoordinator(IVisitorEstimateStore Store, VisitorEstimateValidator Validator, IClock Clock, List<string> Journal, VisitorEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorEstimateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorEstimateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor estimate {code} not found");
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

/// <summary>Functions of visitor estimate workflow.</summary>
public static class VisitorEstimateWorkflow
{
    public static VisitorEstimateCoordinator NewVisitorEstimateCoordinator(IVisitorEstimateStore store, IClock clock)
    {
        return new VisitorEstimateCoordinator(
            store,
            VisitorEstimateRules.DefaultVisitorEstimateValidator(),
            clock,
            new List<string>(),
            VisitorEstimateEngine.DefaultVisitorEstimateEngine());
    }
}
