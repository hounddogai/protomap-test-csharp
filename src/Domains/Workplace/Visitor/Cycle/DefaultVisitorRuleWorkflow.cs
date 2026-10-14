namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default visitor rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultVisitorRuleDesk(IDefaultVisitorRuleStore Store, DefaultVisitorRuleValidator Validator, IClock Clock, List<string> Journal, DefaultVisitorRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default visitor rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultVisitorRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default visitor rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultVisitorRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default visitor rule {code} not found");
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

/// <summary>Functions of default visitor rule workflow.</summary>
public static class DefaultVisitorRuleWorkflow
{
    public static DefaultVisitorRuleDesk NewDefaultVisitorRuleDesk(IDefaultVisitorRuleStore store, IClock clock, Registry flags)
    {
        return new DefaultVisitorRuleDesk(
            store,
            DefaultVisitorRuleRules.DefaultDefaultVisitorRuleValidator(),
            clock,
            new List<string>(),
            DefaultVisitorRuleEngine.DefaultDefaultVisitorRuleEngine(flags));
    }
}
