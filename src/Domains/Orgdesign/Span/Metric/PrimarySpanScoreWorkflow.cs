namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary span scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimarySpanScoreCoordinator(IPrimarySpanScoreStore Store, PrimarySpanScoreValidator Validator, IClock Clock, List<string> Journal, PrimarySpanScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary span score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimarySpanScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary span score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimarySpanScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary span score {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of primary span score workflow.</summary>
public static class PrimarySpanScoreWorkflow
{
    public static PrimarySpanScoreCoordinator NewPrimarySpanScoreCoordinator(IPrimarySpanScoreStore store, IClock clock)
    {
        return new PrimarySpanScoreCoordinator(
            store,
            PrimarySpanScoreRules.DefaultPrimarySpanScoreValidator(),
            clock,
            new List<string>(),
            PrimarySpanScoreEngine.DefaultPrimarySpanScoreEngine());
    }
}
