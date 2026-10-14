namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling certification summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingCertificationSummaryDesk(IRollingCertificationSummaryStore Store, RollingCertificationSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling certification summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingCertificationSummaryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling certification summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingCertificationSummaryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of rolling certification summary workflow.</summary>
public static class RollingCertificationSummaryWorkflow
{
    public static RollingCertificationSummaryDesk NewRollingCertificationSummaryDesk(IRollingCertificationSummaryStore store, IClock clock)
    {
        return new RollingCertificationSummaryDesk(
            store,
            RollingCertificationSummaryRules.DefaultRollingCertificationSummaryValidator(),
            clock,
            new List<string>());
    }
}
