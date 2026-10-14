namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunSegmentDesk(IPayRunSegmentStore Store, PayRunSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunSegmentStatus.Stopped);
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

/// <summary>Functions of pay run segment workflow.</summary>
public static class PayRunSegmentWorkflow
{
    public static PayRunSegmentDesk NewPayRunSegmentDesk(IPayRunSegmentStore store, IClock clock)
    {
        return new PayRunSegmentDesk(
            store,
            PayRunSegmentRules.DefaultPayRunSegmentValidator(),
            clock,
            new List<string>());
    }
}
