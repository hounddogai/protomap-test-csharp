namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary laptop slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryLaptopSlotDesk(IPrimaryLaptopSlotStore Store, PrimaryLaptopSlotValidator Validator, IClock Clock, List<string> Journal, PrimaryLaptopSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary laptop slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryLaptopSlotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary laptop slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryLaptopSlotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary laptop slot {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of primary laptop slot workflow.</summary>
public static class PrimaryLaptopSlotWorkflow
{
    public static PrimaryLaptopSlotDesk NewPrimaryLaptopSlotDesk(IPrimaryLaptopSlotStore store, IClock clock, Registry flags)
    {
        return new PrimaryLaptopSlotDesk(
            store,
            PrimaryLaptopSlotRules.DefaultPrimaryLaptopSlotValidator(),
            clock,
            new List<string>(),
            PrimaryLaptopSlotEngine.DefaultPrimaryLaptopSlotEngine(flags));
    }
}
