namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolPolicyWorkflow(IBonusPoolPolicyStore Store, BonusPoolPolicyValidator Validator, IClock Clock, List<string> Journal, BonusPoolPolicyEngine Engine)
{
    public static BonusPoolPolicyWorkflow NewBonusPoolPolicyWorkflow(IBonusPoolPolicyStore store, IClock clock, Registry flags)
    {
        return new BonusPoolPolicyWorkflow(
            store,
            BonusPoolPolicyRules.DefaultBonusPoolPolicyValidator(),
            clock,
            new List<string>(),
            BonusPoolPolicyEngine.DefaultBonusPoolPolicyEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool policy {code} not found");
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
