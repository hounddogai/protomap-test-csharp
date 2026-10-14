namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal payslip baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalPayslipBaselineService(ISeasonalPayslipBaselineStore Store, SeasonalPayslipBaselineValidator Validator, IClock Clock, List<string> Journal, SeasonalPayslipBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal payslip baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPayslipBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal payslip baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPayslipBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal payslip baseline {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of seasonal payslip baseline workflow.</summary>
public static class SeasonalPayslipBaselineWorkflow
{
    public static SeasonalPayslipBaselineService NewSeasonalPayslipBaselineService(ISeasonalPayslipBaselineStore store, IClock clock, Registry flags)
    {
        return new SeasonalPayslipBaselineService(
            store,
            SeasonalPayslipBaselineRules.DefaultSeasonalPayslipBaselineValidator(),
            clock,
            new List<string>(),
            SeasonalPayslipBaselineEngine.DefaultSeasonalPayslipBaselineEngine(flags));
    }
}
