namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary license seat cycles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryLicenseSeatCycleManager(IPrimaryLicenseSeatCycleStore Store, PrimaryLicenseSeatCycleValidator Validator, IClock Clock, List<string> Journal, PrimaryLicenseSeatCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary license seat cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryLicenseSeatCycleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary license seat cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryLicenseSeatCycleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary license seat cycle {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of primary license seat cycle workflow.</summary>
public static class PrimaryLicenseSeatCycleWorkflow
{
    public static PrimaryLicenseSeatCycleManager NewPrimaryLicenseSeatCycleManager(IPrimaryLicenseSeatCycleStore store, IClock clock, Registry flags)
    {
        return new PrimaryLicenseSeatCycleManager(
            store,
            PrimaryLicenseSeatCycleRules.DefaultPrimaryLicenseSeatCycleValidator(),
            clock,
            new List<string>(),
            PrimaryLicenseSeatCycleEngine.DefaultPrimaryLicenseSeatCycleEngine(flags));
    }
}
