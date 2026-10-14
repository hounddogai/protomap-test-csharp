namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted balance snapshot is in its life cycle.</summary>
public enum WeightedBalanceSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted balance snapshot. Its code is unique within a tenant.</summary>
public sealed class WeightedBalanceSnapshot(string code, string label, int startsAt, string name, string jobCode, string summary, WeightedBalanceSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Summary { get; set; } = summary;
    public WeightedBalanceSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a weighted balance snapshot in status requested.</summary>
    public static WeightedBalanceSnapshot NewWeightedBalanceSnapshot(string code, string label)
    {
        return new WeightedBalanceSnapshot(code, label, 0, "", "", "", WeightedBalanceSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBalanceSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBalanceSnapshotStatus next)
    {
        switch (Status)
        {
            case WeightedBalanceSnapshotStatus.Requested:
                switch (next)
                {
                    case WeightedBalanceSnapshotStatus.Revoked:
                    case WeightedBalanceSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case WeightedBalanceSnapshotStatus.Granted:
                switch (next)
                {
                    case WeightedBalanceSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case WeightedBalanceSnapshotStatus.Revoked:
                switch (next)
                {
                    case WeightedBalanceSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBalanceSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted balance snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public WeightedBalanceSnapshot Copy()
    {
        return new WeightedBalanceSnapshot(Code, Label, StartsAt, Name, JobCode, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
