namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationTemplateManager(IAttestationTemplateStore Store, AttestationTemplateValidator Validator, IClock Clock, List<string> Journal, AttestationTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationTemplateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationTemplateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation template {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of attestation template workflow.</summary>
public static class AttestationTemplateWorkflow
{
    public static AttestationTemplateManager NewAttestationTemplateManager(IAttestationTemplateStore store, IClock clock, Registry flags)
    {
        return new AttestationTemplateManager(
            store,
            AttestationTemplateRules.DefaultAttestationTemplateValidator(),
            clock,
            new List<string>(),
            AttestationTemplateEngine.DefaultAttestationTemplateEngine(flags));
    }
}
