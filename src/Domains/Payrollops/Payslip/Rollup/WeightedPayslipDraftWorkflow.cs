namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted payslip drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedPayslipDraftDesk(IWeightedPayslipDraftStore Store, WeightedPayslipDraftValidator Validator, IClock Clock, List<string> Journal, WeightedPayslipDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted payslip draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedPayslipDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted payslip draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedPayslipDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted payslip draft {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of weighted payslip draft workflow.</summary>
public static class WeightedPayslipDraftWorkflow
{
    public static WeightedPayslipDraftDesk NewWeightedPayslipDraftDesk(IWeightedPayslipDraftStore store, IClock clock, Registry flags)
    {
        return new WeightedPayslipDraftDesk(
            store,
            WeightedPayslipDraftRules.DefaultWeightedPayslipDraftValidator(),
            clock,
            new List<string>(),
            WeightedPayslipDraftEngine.DefaultWeightedPayslipDraftEngine(flags));
    }
}
