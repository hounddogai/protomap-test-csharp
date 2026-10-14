namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled assignment slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledAssignmentSlotDesk(IScheduledAssignmentSlotStore Store, ScheduledAssignmentSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled assignment slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAssignmentSlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled assignment slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAssignmentSlotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of scheduled assignment slot workflow.</summary>
public static class ScheduledAssignmentSlotWorkflow
{
    public static ScheduledAssignmentSlotDesk NewScheduledAssignmentSlotDesk(IScheduledAssignmentSlotStore store, IClock clock)
    {
        return new ScheduledAssignmentSlotDesk(
            store,
            ScheduledAssignmentSlotRules.DefaultScheduledAssignmentSlotValidator(),
            clock,
            new List<string>());
    }
}
