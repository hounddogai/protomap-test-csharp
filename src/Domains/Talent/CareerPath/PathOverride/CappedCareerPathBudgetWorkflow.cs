namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped career path budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedCareerPathBudgetManager(ICappedCareerPathBudgetStore Store, CappedCareerPathBudgetValidator Validator, IClock Clock, List<string> Journal, CappedCareerPathBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped career path budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedCareerPathBudgetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped career path budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedCareerPathBudgetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped career path budget {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of capped career path budget workflow.</summary>
public static class CappedCareerPathBudgetWorkflow
{
    public static CappedCareerPathBudgetManager NewCappedCareerPathBudgetManager(ICappedCareerPathBudgetStore store, IClock clock, Registry flags)
    {
        return new CappedCareerPathBudgetManager(
            store,
            CappedCareerPathBudgetRules.DefaultCappedCareerPathBudgetValidator(),
            clock,
            new List<string>(),
            CappedCareerPathBudgetEngine.DefaultCappedCareerPathBudgetEngine(flags));
    }
}
