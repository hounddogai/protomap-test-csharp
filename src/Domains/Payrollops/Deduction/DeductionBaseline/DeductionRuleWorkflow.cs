namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionRuleManager(IDeductionRuleStore Store, DeductionRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of deduction rule workflow.</summary>
public static class DeductionRuleWorkflow
{
    public static DeductionRuleManager NewDeductionRuleManager(IDeductionRuleStore store, IClock clock)
    {
        return new DeductionRuleManager(
            store,
            DeductionRuleRules.DefaultDeductionRuleValidator(),
            clock,
            new List<string>());
    }
}
