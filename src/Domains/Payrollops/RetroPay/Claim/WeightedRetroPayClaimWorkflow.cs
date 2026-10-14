namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted retro pay claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedRetroPayClaimManager(IWeightedRetroPayClaimStore Store, WeightedRetroPayClaimValidator Validator, IClock Clock, List<string> Journal, WeightedRetroPayClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted retro pay claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedRetroPayClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted retro pay claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedRetroPayClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted retro pay claim {code} not found");
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

/// <summary>Functions of weighted retro pay claim workflow.</summary>
public static class WeightedRetroPayClaimWorkflow
{
    public static WeightedRetroPayClaimManager NewWeightedRetroPayClaimManager(IWeightedRetroPayClaimStore store, IClock clock)
    {
        return new WeightedRetroPayClaimManager(
            store,
            WeightedRetroPayClaimRules.DefaultWeightedRetroPayClaimValidator(),
            clock,
            new List<string>(),
            WeightedRetroPayClaimEngine.DefaultWeightedRetroPayClaimEngine());
    }
}
