namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict pool plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPoolPlanDesk(IStrictPoolPlanStore Store, StrictPoolPlanValidator Validator, IClock Clock, List<string> Journal, StrictPoolPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPoolPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPoolPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict pool plan {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of strict pool plan workflow.</summary>
public static class StrictPoolPlanWorkflow
{
    public static StrictPoolPlanDesk NewStrictPoolPlanDesk(IStrictPoolPlanStore store, IClock clock, Registry flags)
    {
        return new StrictPoolPlanDesk(
            store,
            StrictPoolPlanRules.DefaultStrictPoolPlanValidator(),
            clock,
            new List<string>(),
            StrictPoolPlanEngine.DefaultStrictPoolPlanEngine(flags));
    }
}
