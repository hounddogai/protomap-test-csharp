namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled offboarding rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledOffboardingRollupWorkflow(IScheduledOffboardingRollupStore Store, ScheduledOffboardingRollupValidator Validator, IClock Clock, List<string> Journal, ScheduledOffboardingRollupEngine Engine)
{
    public static ScheduledOffboardingRollupWorkflow NewScheduledOffboardingRollupWorkflow(IScheduledOffboardingRollupStore store, IClock clock, Registry flags)
    {
        return new ScheduledOffboardingRollupWorkflow(
            store,
            ScheduledOffboardingRollupRules.DefaultScheduledOffboardingRollupValidator(),
            clock,
            new List<string>(),
            ScheduledOffboardingRollupEngine.DefaultScheduledOffboardingRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled offboarding rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledOffboardingRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled offboarding rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledOffboardingRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled offboarding rollup {code} not found");
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
            if (item.EndsAt > cutoff)
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
