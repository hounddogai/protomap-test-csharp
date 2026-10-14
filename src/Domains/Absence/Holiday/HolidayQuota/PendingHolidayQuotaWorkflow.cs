namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending holiday quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingHolidayQuotaService(IPendingHolidayQuotaStore Store, PendingHolidayQuotaValidator Validator, IClock Clock, List<string> Journal, PendingHolidayQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending holiday quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingHolidayQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending holiday quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingHolidayQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending holiday quota {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of pending holiday quota workflow.</summary>
public static class PendingHolidayQuotaWorkflow
{
    public static PendingHolidayQuotaService NewPendingHolidayQuotaService(IPendingHolidayQuotaStore store, IClock clock, Registry flags)
    {
        return new PendingHolidayQuotaService(
            store,
            PendingHolidayQuotaRules.DefaultPendingHolidayQuotaValidator(),
            clock,
            new List<string>(),
            PendingHolidayQuotaEngine.DefaultPendingHolidayQuotaEngine(flags));
    }
}
