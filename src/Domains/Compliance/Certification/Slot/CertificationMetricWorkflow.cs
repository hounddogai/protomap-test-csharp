namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationMetricCoordinator(ICertificationMetricStore Store, CertificationMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of certification metric workflow.</summary>
public static class CertificationMetricWorkflow
{
    public static CertificationMetricCoordinator NewCertificationMetricCoordinator(ICertificationMetricStore store, IClock clock)
    {
        return new CertificationMetricCoordinator(
            store,
            CertificationMetricRules.DefaultCertificationMetricValidator(),
            clock,
            new List<string>());
    }
}
