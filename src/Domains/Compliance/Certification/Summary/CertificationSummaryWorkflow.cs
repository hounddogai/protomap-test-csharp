namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationSummaryService(ICertificationSummaryStore Store, CertificationSummaryValidator Validator, IClock Clock, List<string> Journal, CertificationSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification summary {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of certification summary workflow.</summary>
public static class CertificationSummaryWorkflow
{
    public static CertificationSummaryService NewCertificationSummaryService(ICertificationSummaryStore store, IClock clock, Registry flags)
    {
        return new CertificationSummaryService(
            store,
            CertificationSummaryRules.DefaultCertificationSummaryValidator(),
            clock,
            new List<string>(),
            CertificationSummaryEngine.DefaultCertificationSummaryEngine(flags));
    }
}
