namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunPolicyService(IPayRunPolicyStore Store, PayRunPolicyValidator Validator, IClock Clock, List<string> Journal, PayRunPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunPolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunPolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run policy {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of pay run policy workflow.</summary>
public static class PayRunPolicyWorkflow
{
    public static PayRunPolicyService NewPayRunPolicyService(IPayRunPolicyStore store, IClock clock)
    {
        return new PayRunPolicyService(
            store,
            PayRunPolicyRules.DefaultPayRunPolicyValidator(),
            clock,
            new List<string>(),
            PayRunPolicyEngine.DefaultPayRunPolicyEngine());
    }
}
