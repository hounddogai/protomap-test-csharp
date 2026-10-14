namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorCycleWorkflow(IVisitorCycleStore Store, VisitorCycleValidator Validator, IClock Clock, List<string> Journal)
{
    public static VisitorCycleWorkflow NewVisitorCycleWorkflow(IVisitorCycleStore store, IClock clock)
    {
        return new VisitorCycleWorkflow(
            store,
            VisitorCycleRules.DefaultVisitorCycleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
