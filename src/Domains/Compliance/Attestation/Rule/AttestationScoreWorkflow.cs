namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationScoreManager(IAttestationScoreStore Store, AttestationScoreValidator Validator, IClock Clock, List<string> Journal, AttestationScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation score {code} not found");
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

/// <summary>Functions of attestation score workflow.</summary>
public static class AttestationScoreWorkflow
{
    public static AttestationScoreManager NewAttestationScoreManager(IAttestationScoreStore store, IClock clock)
    {
        return new AttestationScoreManager(
            store,
            AttestationScoreRules.DefaultAttestationScoreValidator(),
            clock,
            new List<string>(),
            AttestationScoreEngine.DefaultAttestationScoreEngine());
    }
}
