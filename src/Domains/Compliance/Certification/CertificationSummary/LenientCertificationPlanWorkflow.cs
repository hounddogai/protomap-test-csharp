namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient certification plans through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientCertificationPlanWorkflow(ILenientCertificationPlanStore Store, LenientCertificationPlanValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientCertificationPlanWorkflow NewLenientCertificationPlanWorkflow(ILenientCertificationPlanStore store, IClock clock)
    {
        return new LenientCertificationPlanWorkflow(
            store,
            LenientCertificationPlanRules.DefaultLenientCertificationPlanValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientCertificationPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientCertificationPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
