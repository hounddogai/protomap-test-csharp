namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationNoticeManager(IAttestationNoticeStore Store, AttestationNoticeValidator Validator, IClock Clock, List<string> Journal, AttestationNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of attestation notice workflow.</summary>
public static class AttestationNoticeWorkflow
{
    public static AttestationNoticeManager NewAttestationNoticeManager(IAttestationNoticeStore store, IClock clock, Registry flags)
    {
        return new AttestationNoticeManager(
            store,
            AttestationNoticeRules.DefaultAttestationNoticeValidator(),
            clock,
            new List<string>(),
            AttestationNoticeEngine.DefaultAttestationNoticeEngine(flags));
    }
}
