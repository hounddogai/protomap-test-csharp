namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves laptop checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopCheckpointWorkflow(ILaptopCheckpointStore Store, LaptopCheckpointValidator Validator, IClock Clock, List<string> Journal, LaptopCheckpointEngine Engine)
{
    public static LaptopCheckpointWorkflow NewLaptopCheckpointWorkflow(ILaptopCheckpointStore store, IClock clock, Registry flags)
    {
        return new LaptopCheckpointWorkflow(
            store,
            LaptopCheckpointRules.DefaultLaptopCheckpointValidator(),
            clock,
            new List<string>(),
            LaptopCheckpointEngine.DefaultLaptopCheckpointEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop checkpoint {code} not found");
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
            if (item.ExpiresAt > cutoff)
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
