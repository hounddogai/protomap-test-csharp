namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pool windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolWindowWorkflow(IPoolWindowStore Store, PoolWindowValidator Validator, IClock Clock, List<string> Journal, PoolWindowEngine Engine)
{
    public static PoolWindowWorkflow NewPoolWindowWorkflow(IPoolWindowStore store, IClock clock, Registry flags)
    {
        return new PoolWindowWorkflow(
            store,
            PoolWindowRules.DefaultPoolWindowValidator(),
            clock,
            new List<string>(),
            PoolWindowEngine.DefaultPoolWindowEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolWindowStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolWindowStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool window {code} not found");
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
