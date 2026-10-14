namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskScoreWorkflow(IDeskScoreStore Store, DeskScoreValidator Validator, IClock Clock, List<string> Journal, DeskScoreEngine Engine)
{
    public static DeskScoreWorkflow NewDeskScoreWorkflow(IDeskScoreStore store, IClock clock, Registry flags)
    {
        return new DeskScoreWorkflow(
            store,
            DeskScoreRules.DefaultDeskScoreValidator(),
            clock,
            new List<string>(),
            DeskScoreEngine.DefaultDeskScoreEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk score {code} not found");
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
