namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves holiday milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayMilestoneWorkflow(IHolidayMilestoneStore Store, HolidayMilestoneValidator Validator, IClock Clock, List<string> Journal, HolidayMilestoneEngine Engine)
{
    public static HolidayMilestoneWorkflow NewHolidayMilestoneWorkflow(IHolidayMilestoneStore store, IClock clock, Registry flags)
    {
        return new HolidayMilestoneWorkflow(
            store,
            HolidayMilestoneRules.DefaultHolidayMilestoneValidator(),
            clock,
            new List<string>(),
            HolidayMilestoneEngine.DefaultHolidayMilestoneEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayMilestoneStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayMilestoneStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday milestone {code} not found");
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
