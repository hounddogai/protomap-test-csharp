namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped deduction groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedDeductionGroupService(ICappedDeductionGroupStore Store, CappedDeductionGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped deduction group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedDeductionGroupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped deduction group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedDeductionGroupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of capped deduction group workflow.</summary>
public static class CappedDeductionGroupWorkflow
{
    public static CappedDeductionGroupService NewCappedDeductionGroupService(ICappedDeductionGroupStore store, IClock clock)
    {
        return new CappedDeductionGroupService(
            store,
            CappedDeductionGroupRules.DefaultCappedDeductionGroupValidator(),
            clock,
            new List<string>());
    }
}
