namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualPreferenceDesk(IAccrualPreferenceStore Store, AccrualPreferenceValidator Validator, IClock Clock, List<string> Journal, AccrualPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualPreferenceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualPreferenceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual preference {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of accrual preference workflow.</summary>
public static class AccrualPreferenceWorkflow
{
    public static AccrualPreferenceDesk NewAccrualPreferenceDesk(IAccrualPreferenceStore store, IClock clock)
    {
        return new AccrualPreferenceDesk(
            store,
            AccrualPreferenceRules.DefaultAccrualPreferenceValidator(),
            clock,
            new List<string>(),
            AccrualPreferenceEngine.DefaultAccrualPreferenceEngine());
    }
}
