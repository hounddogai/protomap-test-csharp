namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default holiday assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultHolidayAssignmentCoordinator(IDefaultHolidayAssignmentStore Store, DefaultHolidayAssignmentValidator Validator, IClock Clock, List<string> Journal, DefaultHolidayAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultHolidayAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultHolidayAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday assignment {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of default holiday assignment workflow.</summary>
public static class DefaultHolidayAssignmentWorkflow
{
    public static DefaultHolidayAssignmentCoordinator NewDefaultHolidayAssignmentCoordinator(IDefaultHolidayAssignmentStore store, IClock clock)
    {
        return new DefaultHolidayAssignmentCoordinator(
            store,
            DefaultHolidayAssignmentRules.DefaultDefaultHolidayAssignmentValidator(),
            clock,
            new List<string>(),
            DefaultHolidayAssignmentEngine.DefaultDefaultHolidayAssignmentEngine());
    }
}
