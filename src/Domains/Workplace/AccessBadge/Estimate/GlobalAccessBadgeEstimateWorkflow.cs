namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global access badge estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalAccessBadgeEstimateWorkflow(IGlobalAccessBadgeEstimateStore Store, GlobalAccessBadgeEstimateValidator Validator, IClock Clock, List<string> Journal, GlobalAccessBadgeEstimateEngine Engine)
{
    public static GlobalAccessBadgeEstimateWorkflow NewGlobalAccessBadgeEstimateWorkflow(IGlobalAccessBadgeEstimateStore store, IClock clock, Registry flags)
    {
        return new GlobalAccessBadgeEstimateWorkflow(
            store,
            GlobalAccessBadgeEstimateRules.DefaultGlobalAccessBadgeEstimateValidator(),
            clock,
            new List<string>(),
            GlobalAccessBadgeEstimateEngine.DefaultGlobalAccessBadgeEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global access badge estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalAccessBadgeEstimateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global access badge estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalAccessBadgeEstimateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global access badge estimate {code} not found");
        }
        var result = Engine.Price(item);
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
