namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountPolicyWorkflow(IHeadcountPolicyStore Store, HeadcountPolicyValidator Validator, IClock Clock, List<string> Journal, HeadcountPolicyEngine Engine)
{
    public static HeadcountPolicyWorkflow NewHeadcountPolicyWorkflow(IHeadcountPolicyStore store, IClock clock, Registry flags)
    {
        return new HeadcountPolicyWorkflow(
            store,
            HeadcountPolicyRules.DefaultHeadcountPolicyValidator(),
            clock,
            new List<string>(),
            HeadcountPolicyEngine.DefaultHeadcountPolicyEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountPolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount policy {code} not found");
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
