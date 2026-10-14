namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallExceptionWorkflow(ITownHallExceptionStore Store, TownHallExceptionValidator Validator, IClock Clock, List<string> Journal, TownHallExceptionEngine Engine)
{
    public static TownHallExceptionWorkflow NewTownHallExceptionWorkflow(ITownHallExceptionStore store, IClock clock, Registry flags)
    {
        return new TownHallExceptionWorkflow(
            store,
            TownHallExceptionRules.DefaultTownHallExceptionValidator(),
            clock,
            new List<string>(),
            TownHallExceptionEngine.DefaultTownHallExceptionEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallExceptionStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallExceptionStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall exception {code} not found");
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
