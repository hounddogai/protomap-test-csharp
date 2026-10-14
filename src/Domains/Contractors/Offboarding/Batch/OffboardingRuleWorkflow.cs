namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves offboarding rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingRuleWorkflow(IOffboardingRuleStore Store, OffboardingRuleValidator Validator, IClock Clock, List<string> Journal, OffboardingRuleEngine Engine)
{
    public static OffboardingRuleWorkflow NewOffboardingRuleWorkflow(IOffboardingRuleStore store, IClock clock, Registry flags)
    {
        return new OffboardingRuleWorkflow(
            store,
            OffboardingRuleRules.DefaultOffboardingRuleValidator(),
            clock,
            new List<string>(),
            OffboardingRuleEngine.DefaultOffboardingRuleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding rule {code} not found");
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
