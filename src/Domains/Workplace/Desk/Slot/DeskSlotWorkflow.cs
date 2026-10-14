namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskSlotWorkflow(IDeskSlotStore Store, DeskSlotValidator Validator, IClock Clock, List<string> Journal, DeskSlotEngine Engine)
{
    public static DeskSlotWorkflow NewDeskSlotWorkflow(IDeskSlotStore store, IClock clock, Registry flags)
    {
        return new DeskSlotWorkflow(
            store,
            DeskSlotRules.DefaultDeskSlotValidator(),
            clock,
            new List<string>(),
            DeskSlotEngine.DefaultDeskSlotEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk slot {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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
