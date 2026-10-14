namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual attestation estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualAttestationEstimateCoordinator(IManualAttestationEstimateStore Store, ManualAttestationEstimateValidator Validator, IClock Clock, List<string> Journal, ManualAttestationEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual attestation estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAttestationEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual attestation estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAttestationEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual attestation estimate {code} not found");
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

/// <summary>Functions of manual attestation estimate workflow.</summary>
public static class ManualAttestationEstimateWorkflow
{
    public static ManualAttestationEstimateCoordinator NewManualAttestationEstimateCoordinator(IManualAttestationEstimateStore store, IClock clock, Registry flags)
    {
        return new ManualAttestationEstimateCoordinator(
            store,
            ManualAttestationEstimateRules.DefaultManualAttestationEstimateValidator(),
            clock,
            new List<string>(),
            ManualAttestationEstimateEngine.DefaultManualAttestationEstimateEngine(flags));
    }
}
