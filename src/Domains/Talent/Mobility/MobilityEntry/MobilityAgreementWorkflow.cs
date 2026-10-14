namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityAgreementManager(IMobilityAgreementStore Store, MobilityAgreementValidator Validator, IClock Clock, List<string> Journal, MobilityAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityAgreementStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityAgreementStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility agreement {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of mobility agreement workflow.</summary>
public static class MobilityAgreementWorkflow
{
    public static MobilityAgreementManager NewMobilityAgreementManager(IMobilityAgreementStore store, IClock clock)
    {
        return new MobilityAgreementManager(
            store,
            MobilityAgreementRules.DefaultMobilityAgreementValidator(),
            clock,
            new List<string>(),
            MobilityAgreementEngine.DefaultMobilityAgreementEngine());
    }
}
