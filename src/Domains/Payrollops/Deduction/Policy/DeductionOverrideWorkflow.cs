namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionOverrideWorkflow(IDeductionOverrideStore Store, DeductionOverrideValidator Validator, IClock Clock, List<string> Journal, DeductionOverrideEngine Engine)
{
    public static DeductionOverrideWorkflow NewDeductionOverrideWorkflow(IDeductionOverrideStore store, IClock clock, Registry flags)
    {
        return new DeductionOverrideWorkflow(
            store,
            DeductionOverrideRules.DefaultDeductionOverrideValidator(),
            clock,
            new List<string>(),
            DeductionOverrideEngine.DefaultDeductionOverrideEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction override {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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
