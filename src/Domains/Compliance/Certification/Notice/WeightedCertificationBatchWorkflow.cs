namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted certification batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedCertificationBatchDesk(IWeightedCertificationBatchStore Store, WeightedCertificationBatchValidator Validator, IClock Clock, List<string> Journal, WeightedCertificationBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted certification batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedCertificationBatchStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted certification batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedCertificationBatchStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted certification batch {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of weighted certification batch workflow.</summary>
public static class WeightedCertificationBatchWorkflow
{
    public static WeightedCertificationBatchDesk NewWeightedCertificationBatchDesk(IWeightedCertificationBatchStore store, IClock clock)
    {
        return new WeightedCertificationBatchDesk(
            store,
            WeightedCertificationBatchRules.DefaultWeightedCertificationBatchValidator(),
            clock,
            new List<string>(),
            WeightedCertificationBatchEngine.DefaultWeightedCertificationBatchEngine());
    }
}
