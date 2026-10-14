namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipCheckpointCoordinator(IPayslipCheckpointStore Store, PayslipCheckpointValidator Validator, IClock Clock, List<string> Journal, PayslipCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipCheckpointStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipCheckpointStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of payslip checkpoint workflow.</summary>
public static class PayslipCheckpointWorkflow
{
    public static PayslipCheckpointCoordinator NewPayslipCheckpointCoordinator(IPayslipCheckpointStore store, IClock clock)
    {
        return new PayslipCheckpointCoordinator(
            store,
            PayslipCheckpointRules.DefaultPayslipCheckpointValidator(),
            clock,
            new List<string>(),
            PayslipCheckpointEngine.DefaultPayslipCheckpointEngine());
    }
}
