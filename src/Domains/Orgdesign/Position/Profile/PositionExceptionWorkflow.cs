namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionExceptionCoordinator(IPositionExceptionStore Store, PositionExceptionValidator Validator, IClock Clock, List<string> Journal, PositionExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position exception {code} not found");
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

/// <summary>Functions of position exception workflow.</summary>
public static class PositionExceptionWorkflow
{
    public static PositionExceptionCoordinator NewPositionExceptionCoordinator(IPositionExceptionStore store, IClock clock)
    {
        return new PositionExceptionCoordinator(
            store,
            PositionExceptionRules.DefaultPositionExceptionValidator(),
            clock,
            new List<string>(),
            PositionExceptionEngine.DefaultPositionExceptionEngine());
    }
}
