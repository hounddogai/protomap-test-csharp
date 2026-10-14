namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped attestation windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAttestationWindowWorkflow(ICappedAttestationWindowStore Store, CappedAttestationWindowValidator Validator, IClock Clock, List<string> Journal, CappedAttestationWindowEngine Engine)
{
    public static CappedAttestationWindowWorkflow NewCappedAttestationWindowWorkflow(ICappedAttestationWindowStore store, IClock clock, Registry flags)
    {
        return new CappedAttestationWindowWorkflow(
            store,
            CappedAttestationWindowRules.DefaultCappedAttestationWindowValidator(),
            clock,
            new List<string>(),
            CappedAttestationWindowEngine.DefaultCappedAttestationWindowEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAttestationWindowStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAttestationWindowStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation window {code} not found");
        }
        var result = Engine.Run(item);
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
