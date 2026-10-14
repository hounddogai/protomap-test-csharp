namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipMappingCoordinator(IPayslipMappingStore Store, PayslipMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of payslip mapping workflow.</summary>
public static class PayslipMappingWorkflow
{
    public static PayslipMappingCoordinator NewPayslipMappingCoordinator(IPayslipMappingStore store, IClock clock)
    {
        return new PayslipMappingCoordinator(
            store,
            PayslipMappingRules.DefaultPayslipMappingValidator(),
            clock,
            new List<string>());
    }
}
