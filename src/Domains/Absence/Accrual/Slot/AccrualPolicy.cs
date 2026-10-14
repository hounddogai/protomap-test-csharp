namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual policy is in its life cycle.</summary>
public enum AccrualPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a accrual policy and its current status.</summary>
public sealed class AccrualPolicy(string code, string label, int batchSize, string unit, double minimum, List<string> tags, double ceiling, bool visible, AccrualPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public double Minimum { get; set; } = minimum;
    public List<string> Tags { get; set; } = tags;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public AccrualPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a accrual policy in status created.</summary>
    public static AccrualPolicy NewAccrualPolicy(string code, string label)
    {
        return new AccrualPolicy(
            code,
            label,
            0,
            "",
            0.0,
            new List<string>(),
            0.0,
            false,
            AccrualPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualPolicyStatus next)
    {
        switch (Status)
        {
            case AccrualPolicyStatus.Created:
                switch (next)
                {
                    case AccrualPolicyStatus.Retired:
                        return true;
                }
                return false;
            case AccrualPolicyStatus.Validated:
                switch (next)
                {
                    case AccrualPolicyStatus.Published:
                    case AccrualPolicyStatus.Retired:
                        return true;
                }
                return false;
            case AccrualPolicyStatus.Published:
                switch (next)
                {
                    case AccrualPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"accrual policy"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round(Minimum * (double)BatchSize, 2);
    }

    public AccrualPolicy Copy()
    {
        return new AccrualPolicy(Code, Label, BatchSize, Unit, Minimum, Tags, Ceiling, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
