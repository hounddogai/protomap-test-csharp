namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves license seat exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatExceptionCoordinator(ILicenseSeatExceptionStore Store, LicenseSeatExceptionValidator Validator, IClock Clock, List<string> Journal, LicenseSeatExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat exception {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of license seat exception workflow.</summary>
public static class LicenseSeatExceptionWorkflow
{
    public static LicenseSeatExceptionCoordinator NewLicenseSeatExceptionCoordinator(ILicenseSeatExceptionStore store, IClock clock, Registry flags)
    {
        return new LicenseSeatExceptionCoordinator(
            store,
            LicenseSeatExceptionRules.DefaultLicenseSeatExceptionValidator(),
            clock,
            new List<string>(),
            LicenseSeatExceptionEngine.DefaultLicenseSeatExceptionEngine(flags));
    }
}
