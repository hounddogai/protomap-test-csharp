namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pool notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolNoticeWorkflow(IPoolNoticeStore Store, PoolNoticeValidator Validator, IClock Clock, List<string> Journal, PoolNoticeEngine Engine)
{
    public static PoolNoticeWorkflow NewPoolNoticeWorkflow(IPoolNoticeStore store, IClock clock, Registry flags)
    {
        return new PoolNoticeWorkflow(
            store,
            PoolNoticeRules.DefaultPoolNoticeValidator(),
            clock,
            new List<string>(),
            PoolNoticeEngine.DefaultPoolNoticeEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolNoticeStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolNoticeStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool notice {code} not found");
        }
        var result = Engine.Price(item);
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
