namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskEstimateCoordinator(IDeskEstimateStore Store, DeskEstimateValidator Validator, IClock Clock, List<string> Journal, DeskEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskEstimateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskEstimateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk estimate {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of desk estimate workflow.</summary>
public static class DeskEstimateWorkflow
{
    public static DeskEstimateCoordinator NewDeskEstimateCoordinator(IDeskEstimateStore store, IClock clock)
    {
        return new DeskEstimateCoordinator(
            store,
            DeskEstimateRules.DefaultDeskEstimateValidator(),
            clock,
            new List<string>(),
            DeskEstimateEngine.DefaultDeskEstimateEngine());
    }
}
