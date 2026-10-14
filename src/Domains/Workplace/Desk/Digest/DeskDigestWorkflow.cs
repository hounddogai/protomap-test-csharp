namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskDigestManager(IDeskDigestStore Store, DeskDigestValidator Validator, IClock Clock, List<string> Journal, DeskDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskDigestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskDigestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk digest {code} not found");
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

/// <summary>Functions of desk digest workflow.</summary>
public static class DeskDigestWorkflow
{
    public static DeskDigestManager NewDeskDigestManager(IDeskDigestStore store, IClock clock)
    {
        return new DeskDigestManager(
            store,
            DeskDigestRules.DefaultDeskDigestValidator(),
            clock,
            new List<string>(),
            DeskDigestEngine.DefaultDeskDigestEngine());
    }
}
