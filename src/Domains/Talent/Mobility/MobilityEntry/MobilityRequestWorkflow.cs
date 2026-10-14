namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves mobility requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityRequestDesk(IMobilityRequestStore Store, MobilityRequestValidator Validator, IClock Clock, List<string> Journal, MobilityRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityRequestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityRequestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility request {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of mobility request workflow.</summary>
public static class MobilityRequestWorkflow
{
    public static MobilityRequestDesk NewMobilityRequestDesk(IMobilityRequestStore store, IClock clock, Registry flags)
    {
        return new MobilityRequestDesk(
            store,
            MobilityRequestRules.DefaultMobilityRequestValidator(),
            clock,
            new List<string>(),
            MobilityRequestEngine.DefaultMobilityRequestEngine(flags));
    }
}
