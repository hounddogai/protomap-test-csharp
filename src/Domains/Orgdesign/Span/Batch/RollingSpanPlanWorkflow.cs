namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling span plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingSpanPlanManager(IRollingSpanPlanStore Store, RollingSpanPlanValidator Validator, IClock Clock, List<string> Journal, RollingSpanPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling span plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSpanPlanStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling span plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSpanPlanStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling span plan {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of rolling span plan workflow.</summary>
public static class RollingSpanPlanWorkflow
{
    public static RollingSpanPlanManager NewRollingSpanPlanManager(IRollingSpanPlanStore store, IClock clock, Registry flags)
    {
        return new RollingSpanPlanManager(
            store,
            RollingSpanPlanRules.DefaultRollingSpanPlanValidator(),
            clock,
            new List<string>(),
            RollingSpanPlanEngine.DefaultRollingSpanPlanEngine(flags));
    }
}
