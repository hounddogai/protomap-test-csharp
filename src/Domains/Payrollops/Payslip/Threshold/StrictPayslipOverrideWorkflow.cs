namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict payslip overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPayslipOverrideService(IStrictPayslipOverrideStore Store, StrictPayslipOverrideValidator Validator, IClock Clock, List<string> Journal, StrictPayslipOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict payslip override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPayslipOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict payslip override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPayslipOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict payslip override {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of strict payslip override workflow.</summary>
public static class StrictPayslipOverrideWorkflow
{
    public static StrictPayslipOverrideService NewStrictPayslipOverrideService(IStrictPayslipOverrideStore store, IClock clock)
    {
        return new StrictPayslipOverrideService(
            store,
            StrictPayslipOverrideRules.DefaultStrictPayslipOverrideValidator(),
            clock,
            new List<string>(),
            StrictPayslipOverrideEngine.DefaultStrictPayslipOverrideEngine());
    }
}
