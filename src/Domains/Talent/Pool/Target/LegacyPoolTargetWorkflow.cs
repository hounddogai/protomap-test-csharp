namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy pool targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyPoolTargetDesk(ILegacyPoolTargetStore Store, LegacyPoolTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy pool target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyPoolTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy pool target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyPoolTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of legacy pool target workflow.</summary>
public static class LegacyPoolTargetWorkflow
{
    public static LegacyPoolTargetDesk NewLegacyPoolTargetDesk(ILegacyPoolTargetStore store, IClock clock)
    {
        return new LegacyPoolTargetDesk(
            store,
            LegacyPoolTargetRules.DefaultLegacyPoolTargetValidator(),
            clock,
            new List<string>());
    }
}
