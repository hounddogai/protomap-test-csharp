namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy payslip digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyPayslipDigestDesk(ILegacyPayslipDigestStore Store, LegacyPayslipDigestValidator Validator, IClock Clock, List<string> Journal, LegacyPayslipDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy payslip digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyPayslipDigestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy payslip digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyPayslipDigestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy payslip digest {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of legacy payslip digest workflow.</summary>
public static class LegacyPayslipDigestWorkflow
{
    public static LegacyPayslipDigestDesk NewLegacyPayslipDigestDesk(ILegacyPayslipDigestStore store, IClock clock, Registry flags)
    {
        return new LegacyPayslipDigestDesk(
            store,
            LegacyPayslipDigestRules.DefaultLegacyPayslipDigestValidator(),
            clock,
            new List<string>(),
            LegacyPayslipDigestEngine.DefaultLegacyPayslipDigestEngine(flags));
    }
}
