namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskCycleDesk(IDeskCycleStore Store, DeskCycleValidator Validator, IClock Clock, List<string> Journal, DeskCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskCycleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskCycleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk cycle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of desk cycle workflow.</summary>
public static class DeskCycleWorkflow
{
    public static DeskCycleDesk NewDeskCycleDesk(IDeskCycleStore store, IClock clock)
    {
        return new DeskCycleDesk(
            store,
            DeskCycleRules.DefaultDeskCycleValidator(),
            clock,
            new List<string>(),
            DeskCycleEngine.DefaultDeskCycleEngine());
    }
}
