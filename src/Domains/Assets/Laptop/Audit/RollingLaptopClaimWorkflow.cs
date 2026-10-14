namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling laptop claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingLaptopClaimService(IRollingLaptopClaimStore Store, RollingLaptopClaimValidator Validator, IClock Clock, List<string> Journal, RollingLaptopClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingLaptopClaimStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingLaptopClaimStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop claim {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of rolling laptop claim workflow.</summary>
public static class RollingLaptopClaimWorkflow
{
    public static RollingLaptopClaimService NewRollingLaptopClaimService(IRollingLaptopClaimStore store, IClock clock, Registry flags)
    {
        return new RollingLaptopClaimService(
            store,
            RollingLaptopClaimRules.DefaultRollingLaptopClaimValidator(),
            clock,
            new List<string>(),
            RollingLaptopClaimEngine.DefaultRollingLaptopClaimEngine(flags));
    }
}
