namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped mobility notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedMobilityNoticeWorkflow(ICappedMobilityNoticeStore Store, CappedMobilityNoticeValidator Validator, IClock Clock, List<string> Journal, CappedMobilityNoticeEngine Engine)
{
    public static CappedMobilityNoticeWorkflow NewCappedMobilityNoticeWorkflow(ICappedMobilityNoticeStore store, IClock clock, Registry flags)
    {
        return new CappedMobilityNoticeWorkflow(
            store,
            CappedMobilityNoticeRules.DefaultCappedMobilityNoticeValidator(),
            clock,
            new List<string>(),
            CappedMobilityNoticeEngine.DefaultCappedMobilityNoticeEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped mobility notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedMobilityNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped mobility notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedMobilityNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped mobility notice {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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
