namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationTargetDesk(ICertificationTargetStore Store, CertificationTargetValidator Validator, IClock Clock, List<string> Journal, CertificationTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationTargetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationTargetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification target {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of certification target workflow.</summary>
public static class CertificationTargetWorkflow
{
    public static CertificationTargetDesk NewCertificationTargetDesk(ICertificationTargetStore store, IClock clock, Registry flags)
    {
        return new CertificationTargetDesk(
            store,
            CertificationTargetRules.DefaultCertificationTargetValidator(),
            clock,
            new List<string>(),
            CertificationTargetEngine.DefaultCertificationTargetEngine(flags));
    }
}
