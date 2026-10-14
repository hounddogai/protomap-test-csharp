namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default holiday milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultHolidayMilestoneService(IDefaultHolidayMilestoneStore Store, DefaultHolidayMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultHolidayMilestoneStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultHolidayMilestoneStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of default holiday milestone workflow.</summary>
public static class DefaultHolidayMilestoneWorkflow
{
    public static DefaultHolidayMilestoneService NewDefaultHolidayMilestoneService(IDefaultHolidayMilestoneStore store, IClock clock)
    {
        return new DefaultHolidayMilestoneService(
            store,
            DefaultHolidayMilestoneRules.DefaultDefaultHolidayMilestoneValidator(),
            clock,
            new List<string>());
    }
}
