namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves certification milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationMilestoneCoordinator(ICertificationMilestoneStore Store, CertificationMilestoneValidator Validator, IClock Clock, List<string> Journal, CertificationMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationMilestoneStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationMilestoneStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification milestone {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of certification milestone workflow.</summary>
public static class CertificationMilestoneWorkflow
{
    public static CertificationMilestoneCoordinator NewCertificationMilestoneCoordinator(ICertificationMilestoneStore store, IClock clock, Registry flags)
    {
        return new CertificationMilestoneCoordinator(
            store,
            CertificationMilestoneRules.DefaultCertificationMilestoneValidator(),
            clock,
            new List<string>(),
            CertificationMilestoneEngine.DefaultCertificationMilestoneEngine(flags));
    }
}
