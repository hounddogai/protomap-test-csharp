namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional payslip assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalPayslipAssignmentDesk(IRegionalPayslipAssignmentStore Store, RegionalPayslipAssignmentValidator Validator, IClock Clock, List<string> Journal, RegionalPayslipAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional payslip assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPayslipAssignmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional payslip assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPayslipAssignmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional payslip assignment {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of regional payslip assignment workflow.</summary>
public static class RegionalPayslipAssignmentWorkflow
{
    public static RegionalPayslipAssignmentDesk NewRegionalPayslipAssignmentDesk(IRegionalPayslipAssignmentStore store, IClock clock)
    {
        return new RegionalPayslipAssignmentDesk(
            store,
            RegionalPayslipAssignmentRules.DefaultRegionalPayslipAssignmentValidator(),
            clock,
            new List<string>(),
            RegionalPayslipAssignmentEngine.DefaultRegionalPayslipAssignmentEngine());
    }
}
