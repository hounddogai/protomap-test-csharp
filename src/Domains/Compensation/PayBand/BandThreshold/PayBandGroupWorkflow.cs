namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandGroupService(IPayBandGroupStore Store, PayBandGroupValidator Validator, IClock Clock, List<string> Journal, PayBandGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandGroupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandGroupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band group {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pay band group workflow.</summary>
public static class PayBandGroupWorkflow
{
    public static PayBandGroupService NewPayBandGroupService(IPayBandGroupStore store, IClock clock, Registry flags)
    {
        return new PayBandGroupService(
            store,
            PayBandGroupRules.DefaultPayBandGroupValidator(),
            clock,
            new List<string>(),
            PayBandGroupEngine.DefaultPayBandGroupEngine(flags));
    }
}
