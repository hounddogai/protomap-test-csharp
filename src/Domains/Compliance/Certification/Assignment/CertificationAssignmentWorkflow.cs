namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationAssignmentDesk(ICertificationAssignmentStore Store, CertificationAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of certification assignment workflow.</summary>
public static class CertificationAssignmentWorkflow
{
    public static CertificationAssignmentDesk NewCertificationAssignmentDesk(ICertificationAssignmentStore store, IClock clock)
    {
        return new CertificationAssignmentDesk(
            store,
            CertificationAssignmentRules.DefaultCertificationAssignmentValidator(),
            clock,
            new List<string>());
    }
}
