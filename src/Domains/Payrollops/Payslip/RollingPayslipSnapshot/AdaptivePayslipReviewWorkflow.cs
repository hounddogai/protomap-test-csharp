namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive payslip reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePayslipReviewWorkflow(IAdaptivePayslipReviewStore Store, AdaptivePayslipReviewValidator Validator, IClock Clock, List<string> Journal, AdaptivePayslipReviewEngine Engine)
{
    public static AdaptivePayslipReviewWorkflow NewAdaptivePayslipReviewWorkflow(IAdaptivePayslipReviewStore store, IClock clock, Registry flags)
    {
        return new AdaptivePayslipReviewWorkflow(
            store,
            AdaptivePayslipReviewRules.DefaultAdaptivePayslipReviewValidator(),
            clock,
            new List<string>(),
            AdaptivePayslipReviewEngine.DefaultAdaptivePayslipReviewEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive payslip review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePayslipReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive payslip review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePayslipReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive payslip review {code} not found");
        }
        var result = Engine.Compute(item);
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
