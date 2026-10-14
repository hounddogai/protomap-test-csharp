namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationProposalService(ICertificationProposalStore Store, CertificationProposalValidator Validator, IClock Clock, List<string> Journal, CertificationProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationProposalStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationProposalStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification proposal {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of certification proposal workflow.</summary>
public static class CertificationProposalWorkflow
{
    public static CertificationProposalService NewCertificationProposalService(ICertificationProposalStore store, IClock clock)
    {
        return new CertificationProposalService(
            store,
            CertificationProposalRules.DefaultCertificationProposalValidator(),
            clock,
            new List<string>(),
            CertificationProposalEngine.DefaultCertificationProposalEngine());
    }
}
