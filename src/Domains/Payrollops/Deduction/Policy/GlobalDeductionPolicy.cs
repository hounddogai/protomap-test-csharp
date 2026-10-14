namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global deduction policy is in its life cycle.</summary>
public enum GlobalDeductionPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global deduction policy. Its code is unique within a tenant.</summary>
public sealed class GlobalDeductionPolicy(string code, string name, string country, string channel, string currency, int capacity, int quantity, int sequence, string grade, string region, int createdAt, GlobalDeductionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public string Grade { get; set; } = grade;
    public string Region { get; set; } = region;
    public int CreatedAt { get; set; } = createdAt;
    public GlobalDeductionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a global deduction policy in status new.</summary>
    public static GlobalDeductionPolicy NewGlobalDeductionPolicy(string code, string name)
    {
        return new GlobalDeductionPolicy(
            code,
            name,
            "",
            "",
            "",
            0,
            0,
            0,
            "",
            "",
            0,
            GlobalDeductionPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeductionPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeductionPolicyStatus next)
    {
        switch (Status)
        {
            case GlobalDeductionPolicyStatus.New:
                return next == GlobalDeductionPolicyStatus.Done;
            case GlobalDeductionPolicyStatus.Queued:
                return next == GlobalDeductionPolicyStatus.Running || next == GlobalDeductionPolicyStatus.Failed;
            case GlobalDeductionPolicyStatus.Running:
                return next == GlobalDeductionPolicyStatus.Done || next == GlobalDeductionPolicyStatus.Failed;
            case GlobalDeductionPolicyStatus.Done:
                return next == GlobalDeductionPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalDeductionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global deduction policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)CreatedAt + (double)Quantity, 2);
    }

    public GlobalDeductionPolicy Copy()
    {
        return new GlobalDeductionPolicy(
            Code,
            Name,
            Country,
            Channel,
            Currency,
            Capacity,
            Quantity,
            Sequence,
            Grade,
            Region,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
