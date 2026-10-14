namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskTierWorkflow(IDeskTierStore Store, DeskTierValidator Validator, IClock Clock, List<string> Journal, DeskTierEngine Engine)
{
    public static DeskTierWorkflow NewDeskTierWorkflow(IDeskTierStore store, IClock clock, Registry flags)
    {
        return new DeskTierWorkflow(
            store,
            DeskTierRules.DefaultDeskTierValidator(),
            clock,
            new List<string>(),
            DeskTierEngine.DefaultDeskTierEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskTierStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskTierStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk tier {code} not found");
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
            if (item.CreatedAt > cutoff)
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
