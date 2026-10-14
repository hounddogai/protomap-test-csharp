namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves equity grant records through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantRecordWorkflow(IEquityGrantRecordStore Store, EquityGrantRecordValidator Validator, IClock Clock, List<string> Journal, EquityGrantRecordEngine Engine)
{
    public static EquityGrantRecordWorkflow NewEquityGrantRecordWorkflow(IEquityGrantRecordStore store, IClock clock, Registry flags)
    {
        return new EquityGrantRecordWorkflow(
            store,
            EquityGrantRecordRules.DefaultEquityGrantRecordValidator(),
            clock,
            new List<string>(),
            EquityGrantRecordEngine.DefaultEquityGrantRecordEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantRecordStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantRecordStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant record {code} not found");
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
