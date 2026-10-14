namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default engagement budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultEngagementBudgetWorkflow(IDefaultEngagementBudgetStore Store, DefaultEngagementBudgetValidator Validator, IClock Clock, List<string> Journal, DefaultEngagementBudgetEngine Engine)
{
    public static DefaultEngagementBudgetWorkflow NewDefaultEngagementBudgetWorkflow(IDefaultEngagementBudgetStore store, IClock clock, Registry flags)
    {
        return new DefaultEngagementBudgetWorkflow(
            store,
            DefaultEngagementBudgetRules.DefaultDefaultEngagementBudgetValidator(),
            clock,
            new List<string>(),
            DefaultEngagementBudgetEngine.DefaultDefaultEngagementBudgetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default engagement budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultEngagementBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default engagement budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultEngagementBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default engagement budget {code} not found");
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
