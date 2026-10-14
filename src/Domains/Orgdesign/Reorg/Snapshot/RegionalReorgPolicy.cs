namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional reorg policy is in its life cycle.</summary>
public enum RegionalReorgPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional reorg policy. Its code is unique within a tenant.</summary>
public sealed class RegionalReorgPolicy(string code, string name, int retries, List<string> tags, string grade, string costCenter, string employeeId, string checksum, RegionalReorgPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public string Grade { get; set; } = grade;
    public string CostCenter { get; set; } = costCenter;
    public string EmployeeId { get; set; } = employeeId;
    public string Checksum { get; set; } = checksum;
    public RegionalReorgPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a regional reorg policy in status new.</summary>
    public static RegionalReorgPolicy NewRegionalReorgPolicy(string code, string name)
    {
        return new RegionalReorgPolicy(
            code,
            name,
            0,
            new List<string>(),
            "",
            "",
            "",
            "",
            RegionalReorgPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalReorgPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalReorgPolicyStatus next)
    {
        switch (Status)
        {
            case RegionalReorgPolicyStatus.New:
                return next == RegionalReorgPolicyStatus.Queued || next == RegionalReorgPolicyStatus.Running;
            case RegionalReorgPolicyStatus.Queued:
                return next == RegionalReorgPolicyStatus.Running || next == RegionalReorgPolicyStatus.Done;
            case RegionalReorgPolicyStatus.Running:
                switch (next)
                {
                    case RegionalReorgPolicyStatus.Done:
                        return true;
                }
                return false;
            case RegionalReorgPolicyStatus.Done:
                switch (next)
                {
                    case RegionalReorgPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalReorgPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional reorg policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public RegionalReorgPolicy Copy()
    {
        return new RegionalReorgPolicy(Code, Name, Retries, Tags, Grade, CostCenter, EmployeeId, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
