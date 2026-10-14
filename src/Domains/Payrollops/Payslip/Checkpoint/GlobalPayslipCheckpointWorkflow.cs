namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global payslip checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPayslipCheckpointWorkflow(IGlobalPayslipCheckpointStore Store, GlobalPayslipCheckpointValidator Validator, IClock Clock, List<string> Journal, GlobalPayslipCheckpointEngine Engine)
{
    public static GlobalPayslipCheckpointWorkflow NewGlobalPayslipCheckpointWorkflow(IGlobalPayslipCheckpointStore store, IClock clock)
    {
        return new GlobalPayslipCheckpointWorkflow(
            store,
            GlobalPayslipCheckpointRules.DefaultGlobalPayslipCheckpointValidator(),
            clock,
            new List<string>(),
            GlobalPayslipCheckpointEngine.DefaultGlobalPayslipCheckpointEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global payslip checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPayslipCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global payslip checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPayslipCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global payslip checkpoint {code} not found");
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
