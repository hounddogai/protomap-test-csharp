namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual engagement rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualEngagementRuleWorkflow(IManualEngagementRuleStore Store, ManualEngagementRuleValidator Validator, IClock Clock, List<string> Journal, ManualEngagementRuleEngine Engine)
{
    public static ManualEngagementRuleWorkflow NewManualEngagementRuleWorkflow(IManualEngagementRuleStore store, IClock clock, Registry flags)
    {
        return new ManualEngagementRuleWorkflow(
            store,
            ManualEngagementRuleRules.DefaultManualEngagementRuleValidator(),
            clock,
            new List<string>(),
            ManualEngagementRuleEngine.DefaultManualEngagementRuleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual engagement rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualEngagementRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual engagement rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualEngagementRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual engagement rule {code} not found");
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
