namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionEntryWorkflow(IDeductionEntryStore Store, DeductionEntryValidator Validator, IClock Clock, List<string> Journal)
{
    public static DeductionEntryWorkflow NewDeductionEntryWorkflow(IDeductionEntryStore store, IClock clock)
    {
        return new DeductionEntryWorkflow(
            store,
            DeductionEntryRules.DefaultDeductionEntryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
