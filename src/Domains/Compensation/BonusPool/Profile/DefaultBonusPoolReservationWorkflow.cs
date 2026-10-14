namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default bonus pool reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultBonusPoolReservationWorkflow(IDefaultBonusPoolReservationStore Store, DefaultBonusPoolReservationValidator Validator, IClock Clock, List<string> Journal, DefaultBonusPoolReservationEngine Engine)
{
    public static DefaultBonusPoolReservationWorkflow NewDefaultBonusPoolReservationWorkflow(IDefaultBonusPoolReservationStore store, IClock clock, Registry flags)
    {
        return new DefaultBonusPoolReservationWorkflow(
            store,
            DefaultBonusPoolReservationRules.DefaultDefaultBonusPoolReservationValidator(),
            clock,
            new List<string>(),
            DefaultBonusPoolReservationEngine.DefaultDefaultBonusPoolReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultBonusPoolReservationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultBonusPoolReservationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default bonus pool reservation {code} not found");
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
            if (item.UpdatedAt > cutoff)
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
