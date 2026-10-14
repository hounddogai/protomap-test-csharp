namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient certification tiers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientCertificationTierDesk(ILenientCertificationTierStore Store, LenientCertificationTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientCertificationTierStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient certification tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientCertificationTierStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of lenient certification tier workflow.</summary>
public static class LenientCertificationTierWorkflow
{
    public static LenientCertificationTierDesk NewLenientCertificationTierDesk(ILenientCertificationTierStore store, IClock clock)
    {
        return new LenientCertificationTierDesk(
            store,
            LenientCertificationTierRules.DefaultLenientCertificationTierValidator(),
            clock,
            new List<string>());
    }
}
