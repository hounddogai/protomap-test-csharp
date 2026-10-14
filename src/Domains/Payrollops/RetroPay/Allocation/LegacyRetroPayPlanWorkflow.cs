namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy retro pay plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyRetroPayPlanManager(ILegacyRetroPayPlanStore Store, LegacyRetroPayPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy retro pay plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyRetroPayPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy retro pay plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyRetroPayPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of legacy retro pay plan workflow.</summary>
public static class LegacyRetroPayPlanWorkflow
{
    public static LegacyRetroPayPlanManager NewLegacyRetroPayPlanManager(ILegacyRetroPayPlanStore store, IClock clock)
    {
        return new LegacyRetroPayPlanManager(
            store,
            LegacyRetroPayPlanRules.DefaultLegacyRetroPayPlanValidator(),
            clock,
            new List<string>());
    }
}
