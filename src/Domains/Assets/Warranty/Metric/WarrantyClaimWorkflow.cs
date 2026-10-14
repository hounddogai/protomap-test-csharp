namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyClaimWorkflow(IWarrantyClaimStore Store, WarrantyClaimValidator Validator, IClock Clock, List<string> Journal, WarrantyClaimEngine Engine)
{
    public static WarrantyClaimWorkflow NewWarrantyClaimWorkflow(IWarrantyClaimStore store, IClock clock)
    {
        return new WarrantyClaimWorkflow(
            store,
            WarrantyClaimRules.DefaultWarrantyClaimValidator(),
            clock,
            new List<string>(),
            WarrantyClaimEngine.DefaultWarrantyClaimEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyClaimStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyClaimStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty claim {code} not found");
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
