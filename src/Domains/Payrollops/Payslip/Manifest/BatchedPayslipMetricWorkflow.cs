namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched payslip metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedPayslipMetricService(IBatchedPayslipMetricStore Store, BatchedPayslipMetricValidator Validator, IClock Clock, List<string> Journal, BatchedPayslipMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched payslip metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPayslipMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched payslip metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPayslipMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched payslip metric {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of batched payslip metric workflow.</summary>
public static class BatchedPayslipMetricWorkflow
{
    public static BatchedPayslipMetricService NewBatchedPayslipMetricService(IBatchedPayslipMetricStore store, IClock clock)
    {
        return new BatchedPayslipMetricService(
            store,
            BatchedPayslipMetricRules.DefaultBatchedPayslipMetricValidator(),
            clock,
            new List<string>(),
            BatchedPayslipMetricEngine.DefaultBatchedPayslipMetricEngine());
    }
}
