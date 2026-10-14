namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional attestation schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAttestationScheduleDesk(IRegionalAttestationScheduleStore Store, RegionalAttestationScheduleValidator Validator, IClock Clock, List<string> Journal, RegionalAttestationScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional attestation schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAttestationScheduleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional attestation schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAttestationScheduleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional attestation schedule {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of regional attestation schedule workflow.</summary>
public static class RegionalAttestationScheduleWorkflow
{
    public static RegionalAttestationScheduleDesk NewRegionalAttestationScheduleDesk(IRegionalAttestationScheduleStore store, IClock clock)
    {
        return new RegionalAttestationScheduleDesk(
            store,
            RegionalAttestationScheduleRules.DefaultRegionalAttestationScheduleValidator(),
            clock,
            new List<string>(),
            RegionalAttestationScheduleEngine.DefaultRegionalAttestationScheduleEngine());
    }
}
