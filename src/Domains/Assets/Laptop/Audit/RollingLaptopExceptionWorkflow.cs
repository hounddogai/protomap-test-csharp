namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling laptop exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingLaptopExceptionWorkflow(IRollingLaptopExceptionStore Store, RollingLaptopExceptionValidator Validator, IClock Clock, List<string> Journal, RollingLaptopExceptionEngine Engine)
{
    public static RollingLaptopExceptionWorkflow NewRollingLaptopExceptionWorkflow(IRollingLaptopExceptionStore store, IClock clock, Registry flags)
    {
        return new RollingLaptopExceptionWorkflow(
            store,
            RollingLaptopExceptionRules.DefaultRollingLaptopExceptionValidator(),
            clock,
            new List<string>(),
            RollingLaptopExceptionEngine.DefaultRollingLaptopExceptionEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingLaptopExceptionStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingLaptopExceptionStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling laptop exception {code} not found");
        }
        var result = Engine.Price(item);
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
