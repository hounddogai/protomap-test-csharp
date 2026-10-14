namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditReservationWorkflow(IAuditReservationStore Store, AuditReservationValidator Validator, IClock Clock, List<string> Journal, AuditReservationEngine Engine)
{
    public static AuditReservationWorkflow NewAuditReservationWorkflow(IAuditReservationStore store, IClock clock, Registry flags)
    {
        return new AuditReservationWorkflow(
            store,
            AuditReservationRules.DefaultAuditReservationValidator(),
            clock,
            new List<string>(),
            AuditReservationEngine.DefaultAuditReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit reservation {code} not found");
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
