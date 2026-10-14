namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionBaselineService(IPositionBaselineStore Store, PositionBaselineValidator Validator, IClock Clock, List<string> Journal, PositionBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionBaselineStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionBaselineStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position baseline {code} not found");
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

/// <summary>Functions of position baseline workflow.</summary>
public static class PositionBaselineWorkflow
{
    public static PositionBaselineService NewPositionBaselineService(IPositionBaselineStore store, IClock clock)
    {
        return new PositionBaselineService(
            store,
            PositionBaselineRules.DefaultPositionBaselineValidator(),
            clock,
            new List<string>(),
            PositionBaselineEngine.DefaultPositionBaselineEngine());
    }
}
