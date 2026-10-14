namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped deduction assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedDeductionAssignmentService(ICappedDeductionAssignmentStore Store, CappedDeductionAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped deduction assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedDeductionAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped deduction assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedDeductionAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of capped deduction assignment workflow.</summary>
public static class CappedDeductionAssignmentWorkflow
{
    public static CappedDeductionAssignmentService NewCappedDeductionAssignmentService(ICappedDeductionAssignmentStore store, IClock clock)
    {
        return new CappedDeductionAssignmentService(
            store,
            CappedDeductionAssignmentRules.DefaultCappedDeductionAssignmentValidator(),
            clock,
            new List<string>());
    }
}
