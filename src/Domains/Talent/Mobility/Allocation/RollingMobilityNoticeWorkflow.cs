namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling mobility notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingMobilityNoticeDesk(IRollingMobilityNoticeStore Store, RollingMobilityNoticeValidator Validator, IClock Clock, List<string> Journal, RollingMobilityNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling mobility notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingMobilityNoticeStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling mobility notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingMobilityNoticeStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling mobility notice {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of rolling mobility notice workflow.</summary>
public static class RollingMobilityNoticeWorkflow
{
    public static RollingMobilityNoticeDesk NewRollingMobilityNoticeDesk(IRollingMobilityNoticeStore store, IClock clock, Registry flags)
    {
        return new RollingMobilityNoticeDesk(
            store,
            RollingMobilityNoticeRules.DefaultRollingMobilityNoticeValidator(),
            clock,
            new List<string>(),
            RollingMobilityNoticeEngine.DefaultRollingMobilityNoticeEngine(flags));
    }
}
