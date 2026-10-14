namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunMilestoneDesk(IPayRunMilestoneStore Store, PayRunMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunMilestoneStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunMilestoneStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pay run milestone workflow.</summary>
public static class PayRunMilestoneWorkflow
{
    public static PayRunMilestoneDesk NewPayRunMilestoneDesk(IPayRunMilestoneStore store, IClock clock)
    {
        return new PayRunMilestoneDesk(
            store,
            PayRunMilestoneRules.DefaultPayRunMilestoneValidator(),
            clock,
            new List<string>());
    }
}
