namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction records through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionRecordManager(IDeductionRecordStore Store, DeductionRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionRecordStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of deduction record workflow.</summary>
public static class DeductionRecordWorkflow
{
    public static DeductionRecordManager NewDeductionRecordManager(IDeductionRecordStore store, IClock clock)
    {
        return new DeductionRecordManager(
            store,
            DeductionRecordRules.DefaultDeductionRecordValidator(),
            clock,
            new List<string>());
    }
}
