namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback payslip checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackPayslipCheckpointWorkflow(IFallbackPayslipCheckpointStore Store, FallbackPayslipCheckpointValidator Validator, IClock Clock, List<string> Journal, FallbackPayslipCheckpointEngine Engine)
{
    public static FallbackPayslipCheckpointWorkflow NewFallbackPayslipCheckpointWorkflow(IFallbackPayslipCheckpointStore store, IClock clock, Registry flags)
    {
        return new FallbackPayslipCheckpointWorkflow(
            store,
            FallbackPayslipCheckpointRules.DefaultFallbackPayslipCheckpointValidator(),
            clock,
            new List<string>(),
            FallbackPayslipCheckpointEngine.DefaultFallbackPayslipCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPayslipCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPayslipCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback payslip checkpoint {code} not found");
        }
        var result = Engine.Evaluate(item);
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
