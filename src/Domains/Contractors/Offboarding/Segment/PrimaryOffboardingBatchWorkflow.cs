namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary offboarding batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryOffboardingBatchWorkflow(IPrimaryOffboardingBatchStore Store, PrimaryOffboardingBatchValidator Validator, IClock Clock, List<string> Journal)
{
    public static PrimaryOffboardingBatchWorkflow NewPrimaryOffboardingBatchWorkflow(IPrimaryOffboardingBatchStore store, IClock clock)
    {
        return new PrimaryOffboardingBatchWorkflow(
            store,
            PrimaryOffboardingBatchRules.DefaultPrimaryOffboardingBatchValidator(),
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
            throw new InvalidOperationException($"primary offboarding batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryOffboardingBatchStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary offboarding batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryOffboardingBatchStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
