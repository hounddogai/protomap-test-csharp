namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleNoticeDesk(IMeritCycleNoticeStore Store, MeritCycleNoticeValidator Validator, IClock Clock, List<string> Journal, MeritCycleNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle notice {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of merit cycle notice workflow.</summary>
public static class MeritCycleNoticeWorkflow
{
    public static MeritCycleNoticeDesk NewMeritCycleNoticeDesk(IMeritCycleNoticeStore store, IClock clock)
    {
        return new MeritCycleNoticeDesk(
            store,
            MeritCycleNoticeRules.DefaultMeritCycleNoticeValidator(),
            clock,
            new List<string>(),
            MeritCycleNoticeEngine.DefaultMeritCycleNoticeEngine());
    }
}
