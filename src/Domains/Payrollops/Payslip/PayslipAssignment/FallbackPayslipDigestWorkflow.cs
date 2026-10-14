namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback payslip digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPayslipDigestCoordinator(IFallbackPayslipDigestStore Store, FallbackPayslipDigestValidator Validator, IClock Clock, List<string> Journal, FallbackPayslipDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPayslipDigestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPayslipDigestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip digest {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of fallback payslip digest workflow.</summary>
public static class FallbackPayslipDigestWorkflow
{
    public static FallbackPayslipDigestCoordinator NewFallbackPayslipDigestCoordinator(IFallbackPayslipDigestStore store, IClock clock, Registry flags)
    {
        return new FallbackPayslipDigestCoordinator(
            store,
            FallbackPayslipDigestRules.DefaultFallbackPayslipDigestValidator(),
            clock,
            new List<string>(),
            FallbackPayslipDigestEngine.DefaultFallbackPayslipDigestEngine(flags));
    }
}
