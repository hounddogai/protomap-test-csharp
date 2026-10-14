namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered equity grant records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredEquityGrantRecordDesk(ITieredEquityGrantRecordStore Store, TieredEquityGrantRecordValidator Validator, IClock Clock, List<string> Journal, TieredEquityGrantRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered equity grant record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredEquityGrantRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered equity grant record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredEquityGrantRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered equity grant record {code} not found");
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

/// <summary>Functions of tiered equity grant record workflow.</summary>
public static class TieredEquityGrantRecordWorkflow
{
    public static TieredEquityGrantRecordDesk NewTieredEquityGrantRecordDesk(ITieredEquityGrantRecordStore store, IClock clock)
    {
        return new TieredEquityGrantRecordDesk(
            store,
            TieredEquityGrantRecordRules.DefaultTieredEquityGrantRecordValidator(),
            clock,
            new List<string>(),
            TieredEquityGrantRecordEngine.DefaultTieredEquityGrantRecordEngine());
    }
}
