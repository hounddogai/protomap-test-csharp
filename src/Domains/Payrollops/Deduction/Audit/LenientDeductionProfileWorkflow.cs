namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient deduction profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientDeductionProfileManager(ILenientDeductionProfileStore Store, LenientDeductionProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientDeductionProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientDeductionProfileStatus.Closed);
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

/// <summary>Functions of lenient deduction profile workflow.</summary>
public static class LenientDeductionProfileWorkflow
{
    public static LenientDeductionProfileManager NewLenientDeductionProfileManager(ILenientDeductionProfileStore store, IClock clock)
    {
        return new LenientDeductionProfileManager(
            store,
            LenientDeductionProfileRules.DefaultLenientDeductionProfileValidator(),
            clock,
            new List<string>());
    }
}
