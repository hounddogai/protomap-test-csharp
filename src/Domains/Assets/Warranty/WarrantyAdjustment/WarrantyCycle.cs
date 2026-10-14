namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty cycle is in its life cycle.</summary>
public enum WarrantyCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a warranty cycle and its current status.</summary>
public sealed class WarrantyCycle(string code, string label, string team, int revision, double weight, double hours, int limit, string category, double amount, WarrantyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public int Limit { get; set; } = limit;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public WarrantyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a warranty cycle in status draft.</summary>
    public static WarrantyCycle NewWarrantyCycle(string code, string label)
    {
        return new WarrantyCycle(code, label, "", 0, 0.0, 0.0, 0, "", 0.0, WarrantyCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyCycleStatus next)
    {
        switch (Status)
        {
            case WarrantyCycleStatus.Draft:
                switch (next)
                {
                    case WarrantyCycleStatus.Active:
                    case WarrantyCycleStatus.Closed:
                        return true;
                }
                return false;
            case WarrantyCycleStatus.Pending:
                return next == WarrantyCycleStatus.Suspended || next == WarrantyCycleStatus.Closed;
            case WarrantyCycleStatus.Active:
                return next == WarrantyCycleStatus.Closed;
            case WarrantyCycleStatus.Suspended:
                return next == WarrantyCycleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WarrantyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"warranty cycle"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount - Weight, 2);
    }

    public WarrantyCycle Copy()
    {
        return new WarrantyCycle(Code, Label, Team, Revision, Weight, Hours, Limit, Category, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
